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

	PictureBox imgPanel;

	public ImageSorter(string[] args)
	{
		this.Text = "ImageSorter";
		this.DoubleBuffered = true;
		this.ClientSize = new Size(WIN_WIDTH, WIN_HEIGHT);
		this.FormBorderStyle = FormBorderStyle.FixedSingle;
		this.MaximizeBox = false;
		
		imgPanel = new PictureBox() {
			Size = new Size(WIN_WIDTH, WIN_HEIGHT),
			SizeMode = PictureBoxSizeMode.Zoom,
		};
		
		this.Controls.Add(imgPanel);
		this.BackColor = SystemColors.Window;
		Init();
	}

	public void Init(){
		OpenFolder();
		using(StreamReader sr = new StreamReader("settings.csv")){
			for(int cnt=0; cnt<KEY_NUM; cnt++){
				settings[cnt] = sr.ReadLine();
				string[] temp = settings[cnt].Split(',');
				if (!(Directory.Exists(imgDirPath + "\\" + temp[1])))
				{
					Directory.CreateDirectory(imgDirPath + "\\" + temp[1]);
				}
			}
		}
	}

	//指定したファイルをロックせずにImageを生成する
	public static Image CreateImage(string filename){
		FileStream fs;
		Image img;
		try{
			fs= new FileStream(
				filename,
				FileMode.Open,
				FileAccess.Read
			);
			img = Image.FromStream(fs);
			fs.Close();
		}catch{
			Console.WriteLine(filename + "は画像ファイルではありません");
			return null;
		}
		return img;
	}

	//ディレクトリに仕分けて次を表示
	public void MoveAndNext(string movDirPath){
		if(!isEnd){
			string fileName = Path.GetFileName(files[0]);
			if(files.Count > 1){
				imgPanel.Image =  CreateImage(files[1]);
			}else{
				isEnd = true;
				imgPanel.Image = null;
			}
			File.Move(imgDirPath + "//" + fileName, imgDirPath + "\\" + movDirPath + "\\" + fileName);
			prevMovFile.Add(imgDirPath + "\\" + movDirPath + "\\" + fileName);
			files.Remove(files[0]);
		}
	}

	public void OpenFolder(){
		using(FolderBrowserDialog fbd = new FolderBrowserDialog()){
			if (fbd.ShowDialog(this) == DialogResult.OK){
				imgDirPath = fbd.SelectedPath;
				files = new List<string>(Directory.GetFiles(imgDirPath));
				prevMovFile.Clear();
				imgPanel.Image = CreateImage(files[0]);
				isEnd = false;
			}
		}
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData){
		KeysConverter kc = new KeysConverter();

		if(keyData == (Keys.Control | Keys.O)) {
			OpenFolder();
			return true;
		}

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