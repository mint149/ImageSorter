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

	string imgDirPath;
	List<string> files;
	List<string> prevMovFile = new List<string>();

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
	}

	//指定したファイルをロックせずにImageを生成する
	public static Image CreateImage(string filename)
	{
		FileStream fs = new FileStream(
			filename,
			FileMode.Open,
			FileAccess.Read
		);
		Image img = Image.FromStream(fs);
		fs.Close();
		return img;
	}

	//ディレクトリに仕分けて次を表示
	public void MoveAndNext(string movDirPath){
		if(imgPanel.Image == null){
			return;
		}
		string fileName = Path.GetFileName(files[0]);
		if(files.Count > 1){
			imgPanel.Image =  CreateImage(files[1]);
		}else{
			imgPanel.Image = null;
		}
		File.Move(imgDirPath + "//" + fileName, imgDirPath + "\\" + movDirPath + "\\" + fileName);
		prevMovFile.Add(imgDirPath + "\\" + movDirPath + "\\" + fileName);
		files.Remove(files[0]);
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		//ファイルを開く
		if(keyData == (Keys.Control | Keys.O)) {
			FolderBrowserDialog fbd = new FolderBrowserDialog();

			if (fbd.ShowDialog(this) == DialogResult.OK){
				imgDirPath = fbd.SelectedPath;
				files = new List<string>(Directory.GetFiles(imgDirPath));
				prevMovFile.Clear();
				imgPanel.Image = CreateImage(files[0]);
			}
			fbd.Dispose();
			return true;
		}

		//アンドゥ
		if(keyData == (Keys.Control | Keys.Z)) {
			if(prevMovFile.Count == 0){
				return true;
			}
			string fileName = Path.GetFileName(prevMovFile[prevMovFile.Count - 1]);
			File.Move(prevMovFile[prevMovFile.Count - 1], imgDirPath + "//" + fileName);
			files.Insert(0, imgDirPath + "//" + fileName);
			imgPanel.Image =  CreateImage(files[0]);
			prevMovFile.RemoveAt(prevMovFile.Count - 1);
			return true;
		}

		//各キーで移動
		if(keyData == (Keys.D)) {
			MoveAndNext("Deleted");
			return true;
		}

		if(keyData == (Keys.F)) {
			MoveAndNext("Remained");
			return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

}
