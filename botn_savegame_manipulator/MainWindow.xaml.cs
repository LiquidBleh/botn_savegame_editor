using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace botn_savegame_manipulator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        

        byte[] sexTag_1 = { 0x52, 0x61, 0x63, 0x65, 0x2E, 0x48, 0x75, 0x6D, 0x61, 0x6E, 0x2E, 0x42, 0x72, 0x65, 0x65, 0x64, 0x65, 0x72, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x53, 0x65, 0x78, 0x2E };
        byte[] sexTag_2 = { 0x52, 0x61, 0x63, 0x65, 0x2E, 0x48, 0x75, 0x6D, 0x61, 0x6E, 0x2E, 0x42, 0x72, 0x65, 0x65, 0x64, 0x65, 0x72, 0x00, 0x09, 0x00, 0x00, 0x00, 0x53, 0x65, 0x78, 0x2E };
        byte[][] sexTags; // replace this with wildcard search

        byte[] sexTagNext = { 0x41, 0x70, 0x70, 0x65, 0x61, 0x72, 0x61, 0x6E, 0x63, 0x65, 0x00, 0x0F, 0x00, 0x00, 0x00 };
        byte[] sexPatchFemale = { 0x52, 0x61, 0x63, 0x65, 0x2E, 0x48, 0x75, 0x6D, 0x61, 0x6E, 0x2E, 0x42, 0x72, 0x65, 0x65, 0x64, 0x65
                , 0x72, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x53, 0x65, 0x78, 0x2E, 0x46, 0x65, 0x6D, 0x61, 0x6C, 0x65, 0x00, 0x0B, 0x00, 0x00, 0x00 };
        byte[] sexPatchMale = { 0x52, 0x61, 0x63, 0x65, 0x2E, 0x48, 0x75, 0x6D, 0x61, 0x6E, 0x2E, 0x42, 0x72, 0x65, 0x65, 0x64, 0x65
                , 0x72, 0x00, 0x09, 0x00, 0x00, 0x00, 0x53, 0x65, 0x78, 0x2E, 0x4D, 0x61, 0x6C, 0x65, 0x00, 0x0B, 0x00, 0x00, 0x00 };
        byte[] sexPatchFuta = { 0x52, 0x61, 0x63, 0x65, 0x2E, 0x48, 0x75, 0x6D, 0x61, 0x6E, 0x2E, 0x42, 0x72, 0x65, 0x65, 0x64, 0x65
                , 0x72, 0x00, 0x09, 0x00, 0x00, 0x00, 0x53, 0x65, 0x78, 0x2E, 0x46, 0x75, 0x74, 0x61, 0x00, 0x0B, 0x00, 0x00, 0x00 };

        const String errorText = "Failed to update save game file";
        const String errorOkText = "Successfully updated save game file";

        bool isRunning = false;

        public MainWindow()
        {
            InitializeComponent();

            sexTags = new byte[][] { sexTag_1, sexTag_2 };
        }

        void onSetFemale(object sender, RoutedEventArgs e)
        {
            run(sexPatchFemale);
        }

        void onSetFuta(object sender, RoutedEventArgs e)
        {
            run(sexPatchFuta);
        }

        void onSetMale(object sender, RoutedEventArgs e)
        {
            run(sexPatchMale);
        }

        private void run(byte[] patch)
        {
            if (isRunning)
            {
                return;
            }
            isRunning = true;

            clearErrorLabel();

            var blob = loadSaveFile();
            var newBlob = manipulateBytes(blob, sexTags, sexTagNext, patch);
            if (newBlob != null)
            {
                writeSaveFile(newBlob);
                setErrorLabel(errorOkText);
#pragma warning disable 4014
                triggerClearLabelDelayed();
#pragma warning restore 4014
            }
            else
            {
                setErrorLabel(errorText);
            }
            isRunning = false;
        }

        private byte[] loadSaveFile()
        {
            var fileName = getFileName();
            var blob = System.IO.File.ReadAllBytes(fileName);
            writeBackupSaveFile(blob);
            return blob;
        }

        private void writeBackupSaveFile(byte[] blob)
        {
            var newFile = getFileName() + "_backup_" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
            System.IO.File.WriteAllBytes(newFile, blob);
        }

        private void writeSaveFile(byte[] blob)
        {
            var newFile = getFileName();
            System.IO.File.WriteAllBytes(newFile, blob);
        }

        private byte[] manipulateBytes(byte[] blob, byte[][] tag, byte[] nextTag, byte[] patch)
        {
            int indexTag = -1;
            foreach (var entry in tag)
            {
                indexTag = IndexOf(blob, entry);
                if (indexTag >= 0)
                {
                    break;
                }
            }

            var indexNextTag = IndexOf(blob, nextTag);

            if (indexTag == -1 || indexNextTag == -1)
            {
                return null;
            }

            var result = new byte[blob.Length + 600];
            Buffer.BlockCopy(blob, 0, result, 0, indexTag + tag.Length);
            Buffer.BlockCopy(patch, 0, result, indexTag, patch.Length);
            Buffer.BlockCopy(blob, indexNextTag, result, indexTag + patch.Length, blob.Length - indexNextTag);

            return result;
        }

        private String getFileName()
        {
            var path = saveGamePath.Text;
            if (!path.EndsWith("\\") || !path.EndsWith("/"))
            {
                path += "\\";
            }
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + path + saveGameFile.Text;
        }

        private void clearErrorLabel()
        {
            errorLabel.Content = "";
        }

        private void setErrorLabel(String error)
        {
            errorLabel.Content = error;
        }

        public async Task triggerClearLabelDelayed()
        {
            await Task.Delay(3000);
            clearErrorLabel();
        }

        private int IndexOf(byte[] blob, byte[] tag)
        {
            int hit = 0;
            for (int i = 0; i < blob.Length; i++)
            {
                if (blob[i] == tag[hit])
                {
                    hit++;
                }
                else
                {
                    hit = 0;
                }

                if (tag.Length == hit)
                {
                    return i - tag.Length + 1;
                }
            }
            return -1;
        }
    }
}
