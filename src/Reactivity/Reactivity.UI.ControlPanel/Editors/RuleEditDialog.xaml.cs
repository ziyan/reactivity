using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Reactivity.Objects;
using Reactivity.UI.Collections;
using System.Xml;

namespace Reactivity.UI.ControlPanel.Editors
{
    /// <summary>
    /// Interaction logic for RuleEditDialog.xaml
    /// </summary>
    public partial class RuleEditDialog : Window
    {
        private class Configuration
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        private bool modification = true;
        private Rule rule = null;
        private BackgroundWorker backgroundWorker;
        private ObservableCollection<Configuration> configurations = new DispatchedObservableCollection<Configuration>();

        public RuleEditDialog(Rule rule)
        {
            InitializeComponent();
            this.backgroundWorker = new BackgroundWorker();
            this.backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            this.backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
            if (rule != null)
                this.rule = (Rule)rule.Clone();
            else
            {
                this.rule = new Rule { ID = 0, Name = "<New Rule>", Description = "", Configuration = "", Precedence = 0, IsEnabled = false };
                modification = false;
            }
            //Initialize
            this.RuleID.Text = this.rule.ID.ToString();
            this.RuleName.Text = this.rule.Name;
            this.RuleDescription.Text = this.rule.Description;
            this.RulePrecedence.Text = this.rule.Precedence.ToString();
            this.RuleIsEnabled.IsChecked = this.rule.IsEnabled;
            if (this.rule.Configuration != "" && modification)
            {
                try
                {
                    XmlDocument doc = Util.Xml.Read(this.rule.Configuration);
                    if (doc["rule"]["code"].HasAttribute("path"))
                    {
                        this.RuleAssemblyPath.Text = doc["rule"]["code"].Attributes["path"].Value;
                        this.RuleAssemblyType.Text = doc["rule"]["code"].Attributes["type"].Value;
                        SourceTabItem.IsEnabled = false;
                    }
                    else
                    {
                        this.RuleSourceType.Text = doc["rule"]["code"].Attributes["type"].Value;
                        this.RuleSourceCode.Text = doc["rule"]["code"].InnerText;
                        AssemblyTabItem.IsEnabled = false;
                    }
                    if (doc["rule"].GetElementsByTagName("settings").Count > 0)
                    {
                        XmlNodeList settings_list = doc["rule"]["settings"].GetElementsByTagName("add");
                        for (int j = 0; j < settings_list.Count; j++)
                            configurations.Add(new Configuration { Key = settings_list[j].Attributes["key"].Value, Value = settings_list[j].Attributes["value"].Value });
                    }
                }
                catch
                {
                }
            }

            Binding ConfigurationListBinding = new Binding();
            ConfigurationListBinding.Source = configurations;
            ConfigurationListView.SetBinding(ListView.ItemsSourceProperty, ConfigurationListBinding);
        }
        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((bool)e.Result)
                this.Close();
            else
            {
                MessageBox.Show("An error has occured, failed to execute request.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.IsEnabled = true;
                this.OKButton.Content = "OK";
            }
        }

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Rule rule = (Rule)e.Argument;
            if (modification)
                e.Result = Common.Client.RuleUpdate(rule);
            else
                e.Result = Common.Client.RuleCreate(rule);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.rule.Name = RuleName.Text;
            this.rule.Description = RuleDescription.Text;
            try
            {
                this.rule.Precedence = Convert.ToInt32(RulePrecedence.Text);
            }
            catch
            {
                MessageBox.Show("Precedence can only be an integer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            this.rule.IsEnabled = RuleIsEnabled.IsChecked == true;

            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("rule"));
            XmlElement code = doc.CreateElement("code");
            if(!AssemblyTabItem.IsEnabled)
            {
                if(RuleSourceType.Text == "")
                {
                    MessageBox.Show("Please specify entry type for source code.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (RuleSourceCode.Text=="")
                {
                    MessageBox.Show("Please provide source code.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                code.SetAttribute("type", RuleSourceType.Text);
                code.InnerText = RuleSourceCode.Text;
            }
            else if(!SourceTabItem.IsEnabled)
            {
                if(RuleAssemblyType.Text == "")
                {
                    MessageBox.Show("Please specify entry type for assembly file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (RuleAssemblyPath.Text=="")
                {
                    MessageBox.Show("Please provide path to assembly file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                code.SetAttribute("type", RuleAssemblyType.Text);
                code.SetAttribute("path", RuleAssemblyPath.Text);
            }
            else
            {
                if (RuleSourceType.Text != "" && RuleSourceCode.Text != "")
                {
                    code.SetAttribute("type", RuleSourceType.Text);
                    code.InnerText = RuleSourceCode.Text;
                }
                else if (RuleAssemblyType.Text != "" && RuleAssemblyPath.Text != "")
                {
                    code.SetAttribute("type", RuleAssemblyType.Text);
                    code.SetAttribute("path", RuleAssemblyPath.Text);
                }
                else
                {
                    MessageBox.Show("Please either use assembly file or provide source code.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            doc.DocumentElement.PrependChild(code);

            if (configurations.Count > 0)
            {
                XmlElement settings = doc.CreateElement("settings");
                doc.DocumentElement.PrependChild(settings);
                for (int i = 0; i < configurations.Count; i++)
                {
                    XmlElement add = doc.CreateElement("add");
                    add.SetAttribute("key", configurations[i].Key);
                    add.SetAttribute("value", configurations[i].Value);
                    settings.AppendChild(add);
                }
            }

            this.rule.Configuration = Util.Xml.Write(doc);
            if (backgroundWorker.IsBusy) return;
            this.IsEnabled = false;
            this.OKButton.Content = "Working";
            backgroundWorker.RunWorkerAsync(rule);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                if ((!(e.OriginalSource is TextBox) || !((TextBox)e.OriginalSource).AcceptsReturn) &&
                    !(e.OriginalSource is ComboBox))
                    OKButton_Click(sender, e);
            if (e.Key == Key.Escape)
                CancelButton_Click(sender, e);
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabControl.SelectedItem is TabItem)
            {
                string header = ((TabItem)MainTabControl.SelectedItem).Header.ToString();
                if (header == "Source")
                {
                    this.Width = 800;
                    this.Height = 600;
                    return;
                }
            }
            this.Width = 400;
            this.Height = 500;
        }

        private void ConfigurationAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConfigurationKey.Text == "")
            {
                MessageBox.Show("Key cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfigurationKey.Focus();
                return;
            }
            Configuration newConfig = new Configuration { Key = ConfigurationKey.Text, Value = ConfigurationValue.Text };
            for (int i = 0; i < configurations.Count; i++)
                if (configurations[i].Key == newConfig.Key)
                {
                    MessageBox.Show("Found duplicate key.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ConfigurationKey.Focus();
                    return;
                }
            configurations.Add(newConfig);
            ConfigurationListView.SelectedItem = newConfig;
        }

        private void ConfigurationRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConfigurationListView.SelectedItem is Configuration)
                configurations.Remove((Configuration)ConfigurationListView.SelectedItem);
        }

        private void ConfigurationSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(ConfigurationListView.SelectedItem is Configuration) || ConfigurationListView.SelectedItems.Count != 1)
                return;
            if (ConfigurationKey.Text == "")
            {
                MessageBox.Show("Key cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfigurationKey.Focus();
                return;
            }
            Configuration config = (Configuration)ConfigurationListView.SelectedItem;
            if (config.Key != ConfigurationKey.Text)
            {
                for (int i = 0; i < configurations.Count; i++)
                    if (configurations[i].Key == ConfigurationKey.Text)
                    {
                        MessageBox.Show("Found duplicate key.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        ConfigurationKey.Focus();
                        return;
                    }
                config.Key = ConfigurationKey.Text;
            }
            config.Value = ConfigurationValue.Text;
            configurations.Remove(config);
            configurations.Insert(0, config);
            ConfigurationListView.SelectedItem = config;
        }

        private void ConfigurationListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfigurationRemoveButton.IsEnabled = (ConfigurationListView.SelectedItems.Count > 0);
            if (ConfigurationListView.SelectedItems.Count == 1 && ConfigurationListView.SelectedItem is Configuration) 
            {
                ConfigurationKey.Text = ((Configuration)ConfigurationListView.SelectedItem).Key;
                ConfigurationValue.Text = ((Configuration)ConfigurationListView.SelectedItem).Value;
                ConfigurationSaveButton.IsEnabled = true;
                return;
            }
            ConfigurationKey.Text = "";
            ConfigurationValue.Text = "";
            ConfigurationSaveButton.IsEnabled = false;
        }
    }
}
