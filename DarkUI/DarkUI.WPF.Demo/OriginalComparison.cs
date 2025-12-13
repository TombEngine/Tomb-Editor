using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DarkUI.WPF.Demo;

public partial class OriginalComparison : UserControl
{
	public OriginalComparison()
	{
		InitializeComponent();

		darkButton12.NotifyDefault(true);
		darkTextBox3.BackColor = Color.FromArgb(60, 63, 65);

		darkComboBox1.SelectedIndex = 0;
		darkComboBox2.SelectedIndex = 0;

		// darkSearchableComboBox1.SelectedIndex = 0;
		// darkSearchableComboBox2.SelectedIndex = 0;

		var root1 = new DarkTreeNode("Root 1");
		var root2 = new DarkTreeNode("Root 2");
		var longRoot = new DarkTreeNode("Very long item name which exceeds the width of the TreeView");

		root1.Nodes.Add(new DarkTreeNode("Child 1"));
		root1.Nodes.Add(new DarkTreeNode("Child 2"));
		root1.Nodes.Add(new DarkTreeNode("Child 3"));

		root2.Nodes.Add(new DarkTreeNode("Child 1"));
		root2.Nodes.Add(new DarkTreeNode("Child 2"));
		root2.Nodes.Add(new DarkTreeNode("Child 3"));

		darkTreeView1.Nodes.Add(root1);
		darkTreeView1.Nodes.Add(root2);
		darkTreeView1.Nodes.Add(longRoot);

		darkDataGridView1.Font = new Font("Segoe UI", 8F);

		// Generate random person records
		var people = new List<Person>();
		var random = new Random();

		for (int i = 0; i < 10; i++)
		{
			people.Add(new Person
			{
				FirstName = "First " + i,
				LastName = "Last " + i,
				Age = random.Next(18, 65),
				Country = "Country " + i
			});
		}

		darkDataGridView1.DataSource = people;
	}
}
