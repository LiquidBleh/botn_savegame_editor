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
    public partial class MainWindow : Window
    {
        ////////////////////////////////////////////////////////////////////////////////////////////
        ///
        /// Player Stats
        ///
        ////////////////////////////////////////////////////////////////////////////////////////////   

        byte[] sexTag         = { 0x52, 0x61, 0x63, 0x65, 0x2E, 0x48, 0x75, 0x6D, 0x61, 0x6E, 0x2E, 0x42, 0x72, 0x65, 0x65, 0x64, 0x65, 0x72, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x53, 0x65, 0x78, 0x2E };
        byte[] sexTagMask     = { 0x52, 0x61, 0x63, 0x65, 0x2E, 0x48, 0x75, 0x6D, 0x61, 0x6E, 0x2E, 0x42, 0x72, 0x65, 0x65, 0x64, 0x65, 0x72, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x53, 0x65, 0x78, 0x2E };
        byte[] sexTagNext     = { 0x41, 0x70, 0x70, 0x65, 0x61, 0x72, 0x61, 0x6E, 0x63, 0x65, 0x00, 0x0F, 0x00, 0x00, 0x00 };
        byte[] sexPatchFemale = { 0x52, 0x61, 0x63, 0x65, 0x2E, 0x48, 0x75, 0x6D, 0x61, 0x6E, 0x2E, 0x42, 0x72, 0x65, 0x65, 0x64, 0x65, 0x72, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x53, 0x65, 0x78, 0x2E, 0x46, 0x65, 0x6D, 0x61, 0x6C, 0x65, 0x00, 0x0B, 0x00, 0x00, 0x00 };
        byte[] sexPatchMale   = { 0x52, 0x61, 0x63, 0x65, 0x2E, 0x48, 0x75, 0x6D, 0x61, 0x6E, 0x2E, 0x42, 0x72, 0x65, 0x65, 0x64, 0x65, 0x72, 0x00, 0x09, 0x00, 0x00, 0x00, 0x53, 0x65, 0x78, 0x2E, 0x4D, 0x61, 0x6C, 0x65, 0x00, 0x0B, 0x00, 0x00, 0x00 };
        byte[] sexPatchFuta   = { 0x52, 0x61, 0x63, 0x65, 0x2E, 0x48, 0x75, 0x6D, 0x61, 0x6E, 0x2E, 0x42, 0x72, 0x65, 0x65, 0x64, 0x65, 0x72, 0x00, 0x09, 0x00, 0x00, 0x00, 0x53, 0x65, 0x78, 0x2E, 0x46, 0x75, 0x74, 0x61, 0x00, 0x0B, 0x00, 0x00, 0x00 };
        byte[] spiritFormTag  = { 0x50, 0x6C, 0x61, 0x79, 0x65, 0x72, 0x53, 0x70, 0x69, 0x72, 0x69, 0x74, 0x46, 0x6F, 0x72, 0x6D };

        const String errorText = "Failed to update save game file";
        const String errorOkText = "Successfully updated save game file";

        bool isRunning = false;

        public MainWindow()
        {
            InitializeComponent();

            initPets();
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
            var newBlob = manipulateBytes(blob, sexTag, sexTagMask, sexTagNext, patch);
            if (newBlob != null)
            {
                writeSaveFile(newBlob);
                setErrorLabel(errorOkText);
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

        private byte[] manipulateBytes(byte[] blob, byte[] tag, byte[] mask, byte[] nextTag, byte[] patch)
        {
            int indexTag = int.MaxValue;
            var index = Utils.IndexOf(blob, tag, 0, mask);
            if (index < indexTag && index >= 0)
            {
                indexTag = index;
            }
            if (indexTag == int.MaxValue || indexTag == -1)
            {
                return null;
            }

            var indexNextTag = Utils.IndexOf(blob, nextTag, 0);

            if (indexNextTag == -1)
            {
                return null;
            }

            var result = new byte[blob.Length + 600];
            Buffer.BlockCopy(blob, 0, result, 0, indexTag + tag.Length);
            Buffer.BlockCopy(patch, 0, result, indexTag, patch.Length);
            Buffer.BlockCopy(blob, indexNextTag, result, indexTag + patch.Length, blob.Length - indexNextTag - patch.Length);

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
#pragma warning disable 4014
            triggerClearLabelDelayed();
#pragma warning restore 4014
        }

        public async Task triggerClearLabelDelayed()
        {
            await Task.Delay(3000);
            clearErrorLabel();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////
        ///
        /// Transform
        ///
        ////////////////////////////////////////////////////////////////////////////////////////////

        byte[] breastTransformTag = { 0x00, 0x42, 0x72, 0x65, 0x61, 0x73, 0x74, 0x53, 0x69, 0x7A, 0x65, 0x00, 0x0E, 0x00, 0x00, 0x00, 0x46, 0x6C, 0x6F, 0x61, 0x74, 0x50, 0x72, 0x6F, 0x70, 0x65, 0x72, 0x74, 0x79, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        byte[] breastDepthTag     = { 0x00, 0x42, 0x72, 0x65, 0x61, 0x73, 0x74, 0x44, 0x65, 0x70, 0x74, 0x68, 0x00, 0x0E, 0x00, 0x00, 0x00, 0x46, 0x6C, 0x6F, 0x61, 0x74, 0x50, 0x72, 0x6F, 0x70, 0x65, 0x72, 0x74, 0x79, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        byte[] breastVerticalTag  = { 0x00, 0x42, 0x72, 0x65, 0x61, 0x73, 0x74, 0x56, 0x65, 0x72, 0x74, 0x69, 0x63, 0x61, 0x6C, 0x00, 0x0E, 0x00, 0x00, 0x00, 0x46, 0x6C, 0x6F, 0x61, 0x74, 0x50, 0x72, 0x6F, 0x70, 0x65, 0x72, 0x74, 0x79, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        private float getFloatValue(byte[] blob, int index, int length)
        {
            var floatAsBytes = new byte[4];
            Buffer.BlockCopy(blob, index + length, floatAsBytes, 0, 4);
            Array.Reverse(floatAsBytes, 0, floatAsBytes.Length);
            var value = BitConverter.ToSingle(floatAsBytes, 0);
            byte[] octets = BitConverter.GetBytes(value);
            Array.Reverse(octets, 0, octets.Length);
            value = BitConverter.ToSingle(octets, 0);
            return value;
        }

        private byte[] setFloatValue(byte[] blob, int index, int length, float newValue)
        {
            var newBlob = new byte[blob.Length];

            Buffer.BlockCopy(blob, 0, newBlob, 0, index + length);
            var floatAsBytes = BitConverter.GetBytes(newValue);
            Buffer.BlockCopy(floatAsBytes, 0, newBlob, index + length, 4);
            Buffer.BlockCopy(blob, index + length + 4, newBlob, index + length + 4, blob.Length - (index + length + 4));

            return newBlob;
        }

        private void setterRefresh(byte[] blob, byte[] tag, int offset, TextBox edit)
        {
            if (offset < 0)
            {
                return;
            }
            var indexOfBreastSize = Utils.IndexOf(blob, tag, offset);
            if (indexOfBreastSize >= 0)
            {
                edit.Text = getFloatValue(blob, indexOfBreastSize, tag.Length).ToString();
            }
        }

        void onTransformRefresh(object sender, RoutedEventArgs e)
        {
            var blob = loadSaveFile();

            setterRefresh(blob, breastTransformTag, 0, breastSizeEdit);
            setterRefresh(blob, breastTransformTag, Utils.IndexOf(blob, breastTransformTag, 0) + 1, breastClothedSizeEdit);

            setterRefresh(blob, breastDepthTag, 0, breastDepthEdit);
            setterRefresh(blob, breastDepthTag, Utils.IndexOf(blob, breastDepthTag, 0) + 1, breastDepthClothedEdit);

            setterRefresh(blob, breastVerticalTag, 0, breastVertEdit);
            setterRefresh(blob, breastVerticalTag, Utils.IndexOf(blob, breastVerticalTag, 0) + 1, breastClothedVertEdit);

            setterRefresh(blob, breastTransformTag, Utils.IndexOf(blob, spiritFormTag, 0), spiritBreastSizeEdit);
            setterRefresh(blob, breastDepthTag, Utils.IndexOf(blob, spiritFormTag, 0), spiritBreastDepthEdit);
            setterRefresh(blob, breastVerticalTag, Utils.IndexOf(blob, spiritFormTag, 0), spiritBreastVerticalEdit);

            setErrorLabel("Done.");
        }

        static Func<byte[], int> defaultOffsetCalc = (blob) => { return 0; };

        void setterUpdate(byte[] tag, Func<byte[], int> offsetCalc, String rawInput)
        {
            var blob = loadSaveFile();
            var offset = offsetCalc(blob);
            if (offset == -1)
            {
                return;
            }

            var index = Utils.IndexOf(blob, tag, offset);
            if (index >= 0)
            {
                try
                {
                    var newValue = float.Parse(rawInput);
                    var newBlob = setFloatValue(blob, index, tag.Length, newValue);
                    writeSaveFile(newBlob);
                }
                catch (System.FormatException)
                {
                    setErrorLabel("Invalid input");
                }
            }
            else
            {
                setErrorLabel("Failed to store data");
            }
        }

        void onSetBreastSize(object sender, RoutedEventArgs e)
        {
            setterUpdate(breastTransformTag, defaultOffsetCalc, breastSizeEdit.Text);
        }

        void onSetBreastClothedSize(object sender, RoutedEventArgs e)
        {
            setterUpdate(breastTransformTag, (b) => { return Utils.IndexOf(b, breastTransformTag, 0) + breastTransformTag.Length; }, breastClothedSizeEdit.Text);
        }

        void onSetBreastDepth(object sender, RoutedEventArgs e)
        {
            setterUpdate(breastDepthTag, defaultOffsetCalc, breastDepthEdit.Text);
        }

        void onSetBreastDepthClothed(object sender, RoutedEventArgs e)
        {
            setterUpdate(breastDepthTag, (b) => { return Utils.IndexOf(b, breastDepthTag, 0) + breastDepthTag.Length; }, breastDepthClothedEdit.Text);
        }

        private void onSetBreastVert(object sender, RoutedEventArgs e)
        {
            setterUpdate(breastVerticalTag, defaultOffsetCalc, breastVertEdit.Text);
        }

        private void onSetBreastVertClothed(object sender, RoutedEventArgs e)
        {
            setterUpdate(breastVerticalTag, (b) => { return Utils.IndexOf(b, breastVerticalTag, 0) + breastVerticalTag.Length; }, breastVertEdit.Text);
        }

        private void onSetSpiritBreastSize(object sender, RoutedEventArgs e)
        {
            setterUpdate(breastTransformTag, (b) => { return Utils.IndexOf(b, spiritFormTag, 0) + spiritFormTag.Length; }, spiritBreastSizeEdit.Text);
        }

        private void onSetSpiritBreastDepth(object sender, RoutedEventArgs e)
        {
            setterUpdate(breastDepthTag, (b) => { return Utils.IndexOf(b, spiritFormTag, 0) + spiritFormTag.Length; }, spiritBreastDepthEdit.Text);
        }

        private void onSetSpiritBreastVertical(object sender, RoutedEventArgs e)
        {
            setterUpdate(breastVerticalTag, (b) => { return Utils.IndexOf(b, spiritFormTag, 0) + spiritFormTag.Length; }, spiritBreastVerticalEdit.Text);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////
        ///
        /// Pets
        ///
        ////////////////////////////////////////////////////////////////////////////////////////////

        byte[] petNameTag     = { 0x4E, 0x61, 0x6D, 0x65, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x53, 0x74, 0x72, 0x50, 0x72, 0x6F, 0x70, 0x65, 0x72, 0x74, 0x79, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00 };
        byte[] petNameTagMask = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00 };
        byte[] breederTag     = { 0x52, 0x61, 0x63, 0x65, 0x2E, 0x48, 0x75, 0x6D, 0x61, 0x6E, 0x2E, 0x42, 0x72, 0x65, 0x65, 0x64, 0x65, 0x72 };

        private void initPets()
        {
            petComboBox.SelectionChanged += new SelectionChangedEventHandler(comboBoxUpdated);
        }

        class PetProperties
        {
            public static String PlayerName;

            public String name;
            public int offset;
            public int offsetOfNext;
            public byte[] blob;
        };

        private String readTillNull(byte[] blob, int offset)
        {
            int i = offset;
            while(blob[i] > 0x00)
            {
                ++i;
            }
            return new System.Text.ASCIIEncoding().GetString(blob.Skip(offset).Take(i - offset).ToArray());
        }

        PetProperties selectedPet = null;
        private void comboBoxUpdated(object sender, SelectionChangedEventArgs e)
        {
            selectedPet = (((sender as ComboBox).SelectedItem as ComboBoxItem).Tag as PetProperties);
        }

        private void onPetRefresh(object sender, RoutedEventArgs e)
        {
            petComboBox.Items.Clear();

            var blob = loadSaveFile();

            List<PetProperties> temp = new List<PetProperties>();
            {
                int offset = 0;
                PetProperties p = null;
                while (true)
                {
                    offset = Utils.IndexOf(blob, petNameTag, offset, petNameTagMask);
                    if (offset == -1)
                    {
                        offset = 0;
                        break;
                    }

                    if (p != null)
                    {
                        p.offsetOfNext = offset;
                    }

                    offset += petNameTag.Length;
                    var pet = readTillNull(blob, offset);

                    if (p == null)
                    {
                        PetProperties.PlayerName = pet;
                    }

                    p = new PetProperties();
                    p.name = pet;
                    p.offset = offset;
                    p.blob = blob;
                    temp.Add(p);
                }
            }

            foreach(var entry in temp)
            {
                var item = new ComboBoxItem();
                item.Content = entry.name;
                item.Tag = entry;

                if (Utils.IndexOf(entry.blob, breederTag, entry.offset, null, entry.offsetOfNext) != -1
                    || PetProperties.PlayerName == entry.name)
                {
                    continue;
                }
                petComboBox.Items.Add(item);
            }
        }
    }
}
