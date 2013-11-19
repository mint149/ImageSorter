//画像整理プログラムImageSorter
//Console.WriteLine(fileName);

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

	bool isEnd = true;
	string imgDirPath;
	List<string> files;
	List<string> prevMovFile = new List<string>();
	List<string> settings = new List<string>();
	MenuStrip menu;
	StatusStrip statusBar;
	ToolStripStatusLabel tsslText;
	PictureBox imgPanel;

	public ImageSorter(string[] args){
		this.Text = "ImageSorter";
		this.DoubleBuffered = true;
		this.ClientSize = new Size(WIN_WIDTH, WIN_HEIGHT);
		this.MaximizeBox = false;
		this.FormBorderStyle = FormBorderStyle.Sizable;

		ToolStripMenuItem tsmiOpen = new ToolStripMenuItem("フォルダを開く(&O)");
		tsmiOpen.Click += new EventHandler(tsmiOpen_Click);
		tsmiOpen.ShortcutKeys = Keys.Control | Keys.O;

		ToolStripMenuItem tsmiReload = new ToolStripMenuItem("設定をリロード(&R)");
		tsmiReload.Click += new EventHandler(tsmiReload_Click);
		tsmiReload.ShortcutKeys = Keys.Control | Keys.R;

		ToolStripMenuItem tsmiFile = new ToolStripMenuItem("ファイル(&F)");
		tsmiFile.DropDownItems.AddRange(new ToolStripItem[]{
			tsmiOpen, tsmiReload,
		});

		menu = new MenuStrip();
		menu.Items.Add(tsmiFile);

		tsslText = new ToolStripStatusLabel();

		statusBar = new StatusStrip();
		statusBar.Dock = DockStyle.Bottom;
		statusBar.LayoutStyle = ToolStripLayoutStyle.StackWithOverflow;

		statusBar.Items.Add(tsslText);

		imgPanel = new PictureBox() {
			Location = new Point(0, SystemInformation.MenuHeight),
			Size = new Size(WIN_WIDTH, WIN_HEIGHT - (SystemInformation.MenuHeight * 2)),
			Anchor = (AnchorStyles.Bottom | 
				AnchorStyles.Top | 
				AnchorStyles.Left | 
				AnchorStyles.Right
			),
			SizeMode = PictureBoxSizeMode.Zoom,
		};

		this.Controls.AddRange(new Control[]{
			statusBar,
			menu,
			imgPanel,
		});

		this.MainMenuStrip = menu;

		//最初に設定を読み込む
		tsmiReload_Click(null, null);
	}

	void tsmiOpen_Click(object sender, EventArgs e){
		using(FolderBrowserDialog fbd = new FolderBrowserDialog()){
			if (fbd.ShowDialog(this) == DialogResult.OK){
				imgDirPath = fbd.SelectedPath;
				files = new List<string>(Directory.GetFiles(imgDirPath));
				prevMovFile.Clear();
				showImage();
				tsslText.Text = "";
				tsmiReload_Click(null, null);
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

	//ディレクトリに仕分けて次を表示
	public void MoveAndNext(string movDirPath){
		if(!isEnd){
			if(IsImage(files[0])){
				string fileName = Path.GetFileName(files[0]);
				string source = files[0];
				string dist = imgDirPath + "\\" + movDirPath + "\\" + fileName;

				File.Move(source, dist);
				prevMovFile.Insert(0, dist);
				tsslText.Text = fileName + " moved at " + movDirPath + ".";
			}
			files.RemoveAt(0);
			showImage();
		}
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData){
		KeysConverter kc = new KeysConverter();

		//アンドゥ
		if(keyData == (Keys.Control | Keys.Z)) {
			if (prevMovFile.Count != 0){
				string fileName = Path.GetFileName(prevMovFile[0]);
				string source = prevMovFile[0];
				string dist = imgDirPath + "//" + fileName;

				File.Move(source, dist);
				files.Insert(0, dist);
				tsslText.Text = fileName + " returned at " + imgDirPath + ".";

				showImage();
				prevMovFile.RemoveAt(0);
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
				this.Text = "ImageSorter - " + fileName;
			}catch{
				img = null;
				tsslText.Text = fileName + " is broken or not image.";
			}
		}else{
			img = null;
			tsslText.Text = fileName + " is not image.";
		}
		return img;
	}

	void showImage(){
		while(true){
			if(files.Count == 0){
				isEnd = true;
				imgPanel.Image = CreateImage("img//completed.png");
				this.Text = "ImageSorter - " + imgDirPath + " is Completed!";
				break;
			}else{
				if(IsImage(files[0])){
					imgPanel.Image =  CreateImage(files[0]);
					isEnd = false;
					break;
				}else{
					files.Remove(files[0]);
				}
			}
		}
	}
}
