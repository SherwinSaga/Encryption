using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Encryption
{
    public partial class Form1 : Form
    {
        // encryption marker
        private const string FILE_MARKER = "ENCRYPTED_V1_";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog(this);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog(this);
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            ProcessFile(openFileDialog1.FileName, true);
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            ProcessFile(openFileDialog2.FileName, false);
        }

        private void ProcessFile(string filePath, bool isEncrypting)
        {
            string key = textBox1.Text;
            if (string.IsNullOrEmpty(key))
            {
                MessageBox.Show($"Enter a key for {(isEncrypting ? "encryption" : "decryption")}.");
                return;
            }

            try
            {
                byte[] fileContent = File.ReadAllBytes(filePath);
                byte[] processedBytes;

                if (isEncrypting)
                {
                    if (IsEncrypted(fileContent))
                    {
                        MessageBox.Show("This file is already encrypted.");
                        return;
                    }

                    processedBytes = EncryptData(fileContent, key);
                    MessageBox.Show("File encrypted successfully!");
                }
                else
                {
                    processedBytes = DecryptData(fileContent, key);
                    MessageBox.Show("File decrypted successfully!");
                }

                File.WriteAllBytes(filePath, processedBytes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing file: {ex.Message}");
            }
        }

        private bool IsEncrypted(byte[] data)
        {
            try
            {
                string marker = Encoding.UTF8.GetString(data, 0, FILE_MARKER.Length);
                return marker == FILE_MARKER;
            }
            catch
            {
                return false;
            }
        }

        private byte[] EncryptData(byte[] data, string key)
        {
            byte[] markerBytes = Encoding.UTF8.GetBytes(FILE_MARKER);
            byte[] result = new byte[markerBytes.Length + data.Length];
            markerBytes.CopyTo(result, 0);
            data.CopyTo(result, markerBytes.Length);

            ProcessBytesWithKey(result, markerBytes.Length, key, true);
            return result;
        }

        private byte[] DecryptData(byte[] data, string key)
        {
            ProcessBytesWithKey(data, FILE_MARKER.Length, key, false);

            // remove encryption marker
            byte[] result = new byte[data.Length - FILE_MARKER.Length];

            Array.Copy(data, FILE_MARKER.Length, result, 0, result.Length);
            return result;
        }

        private void ProcessBytesWithKey(byte[] data, int startIndex, string key, bool encrypt)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            int keyLength = keyBytes.Length;

            // vigenere cipher as base but modified to use bytes in order to support different file types
            for (int i = startIndex; i < data.Length; i++)
            {
                int keyByte = keyBytes[(i - startIndex) % keyLength];
                if (encrypt)
                {
                    data[i] = (byte)((data[i] + keyByte) % 256);
                }
                else
                {
                    data[i] = (byte)((data[i] - keyByte + 256) % 256);
                }
            }
        }
    }
}