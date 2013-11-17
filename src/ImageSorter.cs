//画像整理プログラムImageSorter

using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

static class MainFrame {
	[STAThread]
	static void Main(string[] args) {
		ImageSorter imagesorter = new ImageSorter(args);
		Application.EnableVisualStyles();
		Application.Run(imagesorter);
	}
}

public class ImageSorter : Form
{
	const int WIN_WIDTH = 800;
	const int WIN_HEIGHT = 400;
	const int KEY_NUM = 4;

	string imgDirPath;
	List<string> files;
	List<string> prevMovFile = new List<string>();
	string[] settings = new string[KEY_NUM];
	bool isEnd = false;
	MenuStrip ms;
	StatusStrip ss;
	ToolStripStatusLabel tsslText;

	PictureBox imgPanel;

	public ImageSorter(string[] args){
		this.Text = "ImageSorter";
		this.DoubleBuffered = true;
		this.ClientSize = new Size(WIN_WIDTH, WIN_HEIGHT);
		this.FormBorderStyle = FormBorderStyle.FixedSingle;
		this.MaximizeBox = false;

		ToolStripMenuItem tsmiOpen = new ToolStripMenuItem("フォルダを開く(&O)");
		tsmiOpen.Click += new EventHandler(tsmiOpen_Click);
		tsmiOpen.ShortcutKeys = Keys.Control | Keys.O;

		ToolStripMenuItem tsmiReload = new ToolStripMenuItem("設定をリロード(&R)");
		tsmiReload.Click += new EventHandler(tsmiReload_Click);
		tsmiReload.ShortcutKeys = Keys.Control | Keys.R;

		ToolStripMenuItem tsmiFile = new ToolStripMenuItem("ファイル(&F)");
		tsmiFile.DropDownItems.AddRange(new ToolStripItem[]{
			tsmiOpen, new ToolStripSeparator(), tsmiReload
		});

		ms = new MenuStrip();
		ms.Items.Add(tsmiFile);

		tsslText = new ToolStripStatusLabel();

		ss = new StatusStrip();
		ss.Dock = DockStyle.Bottom;
		ss.LayoutStyle = ToolStripLayoutStyle.StackWithOverflow;

		ss.Items.Add(tsslText);

		this.Controls.Add(ss);
		this.Controls.Add(ms);
		this.MainMenuStrip = ms;

		imgPanel = new PictureBox() {
			Size = new Size(WIN_WIDTH, WIN_HEIGHT),
			SizeMode = PictureBoxSizeMode.Zoom,
		};
		
		this.Controls.Add(imgPanel);
		//this.BackColor = SystemColors.Window;

		//最初に設定を読み込む
		tsmiReload_Click(null, null);
	}

	public bool IsImage(string fileName){
		string[] extension = new string[] { ".jpg", ".jpeg ", ".png", ".svg", ".tiff", ".tif", "bmp", ".jp2", ".j2c", "dib", ".jxr", ".hdp", ".wdp" };
		string fileEx = Path.GetExtension(fileName);
		foreach (string ex in extension){
			if (string.Compare(ex, fileEx, true) == 0){
				return true;
			}
		}
		return false;
	}

	void tsmiOpen_Click(object sender, EventArgs e){
		using(FolderBrowserDialog fbd = new FolderBrowserDialog()){
			if (fbd.ShowDialog(this) == DialogResult.OK){
				imgDirPath = fbd.SelectedPath;
				files = new List<string>(Directory.GetFiles(imgDirPath));
				prevMovFile.Clear();
				imgPanel.Image = CreateImage(files[0]);
				isEnd = false;
				tsslText.Text = "";
			}
		}
	}

	void tsmiReload_Click(object sender, EventArgs e){
		using(StreamReader sr = new StreamReader("settings.csv")){
			for(int cnt=0; cnt<KEY_NUM; cnt++){
				settings[cnt] = sr.ReadLine();
				string[] temp = settings[cnt].Split(',');
				if (!(Directory.Exists(imgDirPath + "\\" + temp[1]))){
					Directory.CreateDirectory(imgDirPath + "\\" + temp[1]);
				}
			}
		}
	}

	//指定したファイルをロックせずにImageを生成する
	public Image CreateImage(string fileName){
		FileStream fs;
		Image img;
		if(IsImage(fileName)){
			try{
				fs= new FileStream(
					fileName,
					FileMode.Open,
					FileAccess.Read
				);
				img = Image.FromStream(fs);
				fs.Close();
			}catch{
				img = null;
			}
		}else{
			tsslText.Text = fileName + " is not image.";
			img = null;
		}
		if(fileName == "img//completed.png"){
			this.Text = "ImageSorter - " + imgDirPath + " is Completed!";
		}else{
			this.Text = "ImageSorter - " + fileName;
		}
		return img;
	}

	//ディレクトリに仕分けて次を表示
	public void MoveAndNext(string movDirPath){
		if(!isEnd){
			string fileName = Path.GetFileName(files[0]);
			if(IsImage(fileName)){
				File.Move(imgDirPath + "//" + fileName, imgDirPath + "\\" + movDirPath + "\\" + fileName);
				prevMovFile.Add(imgDirPath + "\\" + movDirPath + "\\" + fileName);
				tsslText.Text = fileName + " moved at " + movDirPath + ".";
			}
			if(files.Count > 1){
				imgPanel.Image =  CreateImage(files[1]);
			}else{
				isEnd = true;
				imgPanel.Image = CreateImage("img//completed.png");
			}
			files.Remove(files[0]);
		}
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData){
		KeysConverter kc = new KeysConverter();

		//アンドゥ
		if(keyData == (Keys.Control | Keys.Z)) {
			if (prevMovFile.Count != 0){
				int last = prevMovFile.Count - 1;
				string fileName = Path.GetFileName(prevMovFile[last]);
				File.Move(prevMovFile[last], imgDirPath + "//" + fileName);
				files.Insert(0, imgDirPath + "//" + fileName);
				imgPanel.Image = CreateImage(files[0]);
				prevMovFile.RemoveAt(last);
			}
			return true;
		}

		//各キーでフォルダ移動
		for (int cnt = 0; cnt < KEY_NUM; cnt++){
			string[] temp = settings[cnt].Split(',');
			if (keyData == ((Keys)kc.ConvertFromString(temp[0]))){
				MoveAndNext(temp[1]);
				return true;
			}
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}
}