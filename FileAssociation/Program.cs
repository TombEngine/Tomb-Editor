using MiniFileAssociation;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FileAssociation
{
	internal static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			if (args.Length > 0)
			{
				if (args[0].Equals("-a", StringComparison.OrdinalIgnoreCase))
					RunAssociation();
				else if (Regex.IsMatch(args[0], @"-\d\d\d"))
					RunAssociationWithBinaryArgs(args[0]);
				else if (args[0].Equals("-d", StringComparison.OrdinalIgnoreCase))
					RunDeassociation();
			}
			else
				OpenGUI();
		}

		private static void OpenGUI()
		{
			Application.EnableVisualStyles();
			Application.SetDefaultFont(new System.Drawing.Font("Segoe UI", 8.25f));
			Application.SetHighDpiMode(HighDpiMode.DpiUnawareGdiScaled);
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new FormMain());
		}

		private static void RunAssociation()
		{
			AssociatePRJ2();
			AssociateWAD2();
			AssociateTRPROJ();
			AssociatePRJ();
			AssociateWAD();
		}

		private static void RunAssociationWithBinaryArgs(string args)
		{
			bool associatePRJ2 = int.Parse(args[1].ToString()) != 0;
			bool associateWAD2 = int.Parse(args[2].ToString()) != 0;
			bool associateTRPROJ = int.Parse(args[3].ToString()) != 0;
			bool associatePRJ = int.Parse(args[4].ToString()) != 0;
			bool associateWAD = int.Parse(args[5].ToString()) != 0;

			if (associatePRJ2)
				AssociatePRJ2();

			if (associateWAD2)
				AssociateWAD2();

			if (associateTRPROJ)
				AssociateTRPROJ();

			if (associatePRJ)
				AssociatePRJ();

			if (associateWAD)
				AssociateWAD();
		}

		private static void RunDeassociation() // Is this even a real word?
		{
			Association.ClearAssociations(".prj2");
			Association.ClearAssociations(".wad2");
			Association.ClearAssociations(".trproj");
			Association.ClearAssociations(".prj");
			Association.ClearAssociations(".wad");
		}

		public static void AssociatePRJ2()
		{
			string extension = ".prj2";
			string openWith = DefaultPaths.TombEditorExecutable;
			string description = "TombEditor Project File";
			string iconPath = Path.Combine(DefaultPaths.ResourcesDirectory, "te_file.ico");

			Association.SetAssociation(extension, openWith, description, iconPath);
		}

		public static void AssociateWAD2()
		{
			string extension = ".wad2";
			string openWith = DefaultPaths.WadToolExecutable;
			string description = "Wad2 Object File";
			string iconPath = Path.Combine(DefaultPaths.ResourcesDirectory, "wt_file.ico");

			Association.SetAssociation(extension, openWith, description, iconPath);
		}

		public static void AssociateTRPROJ()
		{
			string extension = ".trproj";
			string openWith = DefaultPaths.TombIDEExecutable;
			string description = "TombIDE Project File";
			string iconPath = Path.Combine(DefaultPaths.ResourcesDirectory, "tide_file.ico");

			Association.SetAssociation(extension, openWith, description, iconPath);
		}

		public static void AssociatePRJ()
		{
			string extension = ".prj";
			string openWith = DefaultPaths.TombEditorExecutable;
			string description = "TRLE Project File";
			string iconPath = Path.Combine(DefaultPaths.ResourcesDirectory, "te_file.ico");

			Association.SetAssociation(extension, openWith, description, iconPath);
		}

		public static void AssociateWAD()
		{
			string extension = ".wad";
			string openWith = DefaultPaths.WadToolExecutable;
			string description = "TRLE Wad Object File";
			string iconPath = Path.Combine(DefaultPaths.ResourcesDirectory, "wt_file.ico");

			Association.SetAssociation(extension, openWith, description, iconPath);
		}
	}
}
