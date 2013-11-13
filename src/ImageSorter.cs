/*
画像整理プログラムImageSorter
･ASDFJKL;にフォルダを割り当て，押した瞬間に見てる画像をフォルダに移動，次の画像を表示する．TODO
･アンドゥ対応TODO
*/
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
		Application.Run(window);
	}
}

public class ImageSorter : Form
{
	const int WIN_WIDTH = 800;
	const int WIN_HEIGHT = 400;

	string imgDirPath;
	List<string> files;

	PictureBox imgPanel;

	public Window(string[] args)
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
		files.Remove(files[0]);
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		//Ctrl+Oでファイルを開く
		if(keyData == (Keys.Control | Keys.O)) {
			//FolderBrowserDialogクラスのインスタンスを作成
			FolderBrowserDialog fbd = new FolderBrowserDialog();

			//ダイアログを表示する
			if (fbd.ShowDialog(this) == DialogResult.OK)
			{
				//選択されたフォルダを表示する
				imgDirPath = fbd.SelectedPath;
				files = new List<string>(Directory.GetFiles(imgDirPath));
				Console.WriteLine(files[0]);
				imgPanel.Image = CreateImage(files[0]);
			}
			fbd.Dispose();
			return true;
		}
		// DでDeletedに移動
		if(keyData == (Keys.D)) {
			MoveAndNext("Deleted");
			return true;
		}
		// FでRemainedに移動
		if(keyData == (Keys.F)) {
			MoveAndNext("Remained");
			return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

}
