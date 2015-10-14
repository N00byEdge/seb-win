﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media;
using MetroFramework;
using MetroFramework.Controls;
using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.Properties;
using SebWindowsClient.XULRunnerCommunication;
using Color = System.Drawing.Color;
using ListObj = System.Collections.Generic.List<object>;
using DictObj = System.Collections.Generic.Dictionary<string, object>;

namespace SebWindowsClient.UI
{
    public class SEBAdditionalResourcesToolStripButton : SEBToolStripButton
    {
        private ContextMenuStrip _menu;
        private IFileCompressor _fileCompressor;

        public SEBAdditionalResourcesToolStripButton()
        {
            InitializeComponent();

            _menu = new ContextMenuStrip();
            _fileCompressor = new FileCompressor();

            LoadItems();
        }

        private void InitializeComponent()
        {
            base.Image = Resources.quit;
        }

        private void LoadItems()
        {
            foreach (DictObj l0resource in ((ListObj)SEBSettings.settingsCurrent[SEBSettings.KeyAdditionalResources]))
            {
                if (!(bool)l0resource[SEBSettings.KeyAdditionalResourcesActive])
                    continue;

                var l0item = new SEBAdditionalResourceMenuItem();
                l0item.Text = l0resource[SEBSettings.KeyAdditionalResourcesTitle].ToString();
                l0item.Resource = l0resource;
                l0item.Click += OnItemClicked;
                AutoOpenResource(l0resource);
                l0item.Image = GetResourceIcon(l0resource);

                foreach (DictObj l1resource in ((ListObj)l0resource[SEBSettings.KeyAdditionalResources]))
                {
                    if (!(bool)l1resource[SEBSettings.KeyAdditionalResourcesActive])
                        continue;

                    var l1item = new SEBAdditionalResourceMenuItem();
                    l1item.Text = l1resource[SEBSettings.KeyAdditionalResourcesTitle].ToString();
                    l1item.Resource = l1resource;
                    l1item.Click += OnItemClicked;
                    AutoOpenResource(l1resource);

                    foreach (DictObj l2resource in ((ListObj)l1resource[SEBSettings.KeyAdditionalResources]))
                    {
                        if (!(bool)l0resource[SEBSettings.KeyAdditionalResourcesActive])
                            continue;

                        var l2item = new SEBAdditionalResourceMenuItem();
                        l2item.Text = l2resource[SEBSettings.KeyAdditionalResourcesTitle].ToString();
                        l2item.Resource = l2resource;
                        l2item.Click += OnItemClicked;
                        AutoOpenResource(l2resource);

                        l1item.DropDownItems.Add(l2item);
                    }
                    l0item.DropDownItems.Add(l1item);
                }
                _menu.Items.Add(l0item);
            }
        }

        private void OnItemClicked(object sender, EventArgs e)
        {
            var item = sender as SEBAdditionalResourceMenuItem;

            if (item != null && item.Resource != null)
            {
                if (!string.IsNullOrEmpty((string) item.Resource[SEBSettings.KeyAdditionalResourcesResourceData]))
                {
                    OpenEmbededResource(item.Resource);
                }
                else if (!string.IsNullOrEmpty((string)item.Resource[SEBSettings.KeyAdditionalResourcesUrl]))
                {
                    SEBXULRunnerWebSocketServer.SendMessage(new SEBXULMessage(SEBXULMessage.SEBXULHandler.AdditionalResources, new { id = item.Resource[SEBSettings.KeyAdditionalResourcesIdentifier] }));
                }
            }
        }

        private Image GetResourceIcon(DictObj resource)
        {
            if (((ListObj)resource[SEBSettings.KeyAdditionalResourcesResourceIcons]).Count > 0)
            {
                var icon =
                    (DictObj)((ListObj)resource[SEBSettings.KeyAdditionalResourcesResourceIcons])[0];
                var memoryStream =
                    new MemoryStream(
                        _fileCompressor.DeCompressAndDecode(
                            (string)icon[SEBSettings.KeyAdditionalResourcesResourceIconsIconData]));
                return Image.FromStream(memoryStream);
            }

            return Resources.SebGlobalDialogImage;
        }

        private void OpenEmbededResource(DictObj resource)
        {
            var launcher = (int) resource[SEBSettings.KeyAdditionalResourcesResourceDataLauncher];
            var filename = (string) resource[SEBSettings.KeyAdditionalResourcesResourceDataFilename];
            var path =
                _fileCompressor.DecompressDecodeAndSaveFile(
                    (string) resource[SEBSettings.KeyAdditionalResourcesResourceData], filename);
            //XulRunner
            if (launcher == 0)
            {
                SEBXULRunnerWebSocketServer.SendMessage(
                    new SEBXULMessage(
                        SEBXULMessage.SEBXULHandler.AdditionalResources, new
                        {
                            id = resource[SEBSettings.KeyAdditionalResourcesIdentifier], 
                            path = path
                        }
                    )
                );
            }
            else
            {
                var permittedProcess = (DictObj)SEBSettings.permittedProcessList[launcher];
                var fullPath = SEBClientInfo.SebWindowsClientForm.GetPermittedApplicationPath(permittedProcess);
                try
                {
                    Process.Start(fullPath, "\"" + path + filename + "\"");
                }
                catch (Exception ex)
                {
                    SEBMessageBox.Show("Error", ex.Message, MessageBoxIcon.Error, MessageBoxButtons.OK);
                }
            }
        }

        private void AutoOpenResource(DictObj resource)
        {
            if ((bool) resource[SEBSettings.KeyAdditionalResourcesAutoOpen] && !string.IsNullOrEmpty((string)resource[SEBSettings.KeyAdditionalResourcesResourceData]))
            {
                OpenEmbededResource(resource);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            _menu.Show(this.Parent,new Point(this.Bounds.X,this.Bounds.Y));
        }
       
    }

}