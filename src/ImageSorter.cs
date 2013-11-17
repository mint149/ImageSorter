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

public class ImageSorter : Form{
	const int WIN_WIDTH = 800;
	const int WIN_HEIGHT = 400;

	string imgDirPath;
	List<string> files;
	List<string> prevMovFile = new List<string>();
	List<string> settings = new List<string>();
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
		bool isIgnore = true;
		foreach (string imageEx in extension){
			if (string.Compare(imageEx, fileEx, isIgnore) == 0){
				return true;
			}
		}
		return false;
	}

	void tsmiOpen_Click(object sender, EventArgs e){
		using(FolderBrowserDialog fbd = new FolderBrowserDialog()){
			if (fbd.ShowDialog(this) == DialogResult.OK){
				isEnd = false;
				imgDirPath = fbd.SelectedPath;
				files = new List<string>(Directory.GetFiles(imgDirPath));
				prevMovFile.Clear();
				showImage();
				tsslText.Text = "";
			}
		}
	}

	void tsmiReload_Click(object sender, EventArgs e){
		using(StreamReader sr = new StreamReader("settings.csv")){
			string temp;
			while((temp = sr.ReadLine()) != null){
				settings.Add(temp);
				string movDirPath = imgDirPath + "\\" + temp.Split(',')[1];
				if (!(Directory.Exists(movDirPath))){
					Directory.CreateDirectory(movDirPath);
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
			files.Remove(files[0]);
			showImage();
		}
	}

	void showImage(){
		while(true){
			if(files.Count == 0){
				isEnd = true;
				imgPanel.Image = CreateImage("img//completed.png");
				break;
			}
			if(IsImage(files[0])){
				imgPanel.Image =  CreateImage(files[0]);
				break;
			}else{
				files.Remove(files[0]);
			}
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
				isEnd = false;
				tsslText.Text = fileName + " returned at " + imgDirPath + ".";
			}
			return true;
		}

		//各キーでフォルダ移動
		foreach(string setting in settings){
			string keyString = setting.Split(',')[0];
			string movDirPath = setting.Split(',')[1];
			if (keyData == ((Keys)kc.ConvertFromString(keyString))){
				MoveAndNext(movDirPath);
				return true;
			}
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}
}