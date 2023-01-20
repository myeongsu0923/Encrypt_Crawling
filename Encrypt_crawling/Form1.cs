using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Encrypt_crawling
{
    public partial class EncryptForm : MetroFramework.Forms.MetroForm
    {
        protected ChromeDriverService _driverService = null;
        protected ChromeOptions _options = null;
        protected ChromeDriver _driver = null;

        protected byte[] SALT = new byte[] { 0x43, 0x87, 0x23, 0x72 };

        public EncryptForm()
        {
            InitializeComponent();

            _driverService = ChromeDriverService.CreateDefaultService();
            _driverService.HideCommandPromptWindow = true;

            _options = new ChromeOptions();
            _options.AddArgument("disable-gpu");
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string drivePath = "C:\\Download\\Image\\";
            string directoryPath = @"C:\Download\Image";
            string zipPath = directoryPath + ".zip";

            DirectoryInfo directory = new DirectoryInfo(drivePath);

            if (directory.Exists == false)
                directory.Create();

            if (cbVisible.Checked == true)
                _options.AddArgument("--headless");

            int count = Convert.ToInt32(tbCount.Text);
            int current = 0;
            string search = tbSearch.Text;

            IWebDriver _driver = new ChromeDriver(_driverService, _options);

            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            _driver.Url = "https://www.google.com/imghp";

            var element = _driver.FindElement(By.XPath("/html/body/div[1]/div[3]/form/div[1]/div[1]/div[1]/div/div[2]/input"));
            element.Click();
            element.SendKeys(search);
            element.SendKeys(OpenQA.Selenium.Keys.Enter);

            IList<IWebElement> all = _driver.FindElements(By.CssSelector("img.rg_i.Q4LuWd"));
            foreach (IWebElement elements in all)
            {
                string driveFile = drivePath + "Image" + current + ".jpg";
                elements.Click();
                Thread.Sleep(500);
                Screenshot _screenshot = ((ITakesScreenshot)elements).GetScreenshot();
                _screenshot.SaveAsFile(driveFile, ScreenshotImageFormat.Jpeg);
                current++;
                lbStatus.Text = current.ToString() + " Image Download";

                if (current >= count)
                {
                    MessageBox.Show("Crawling OK");

                    bool zipResult = ZipDirectory(directoryPath, zipPath);
                    

                    if (zipResult == true) lbStatus.Text += " , Zip OK";
                    else lbStatus.Text += " , Zip Fail";
                    directory.Delete(true);
                    _driver.Close();

                    break;
                }
            }
        }

        public byte[] Encrypt(byte[] input, string password, byte[] salt)
        {
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, salt);
            using (MemoryStream ms = new MemoryStream())
            using (Aes aes = new AesManaged())
            {
                aes.Key = pdb.GetBytes(aes.KeySize / 8);
                aes.IV = pdb.GetBytes(aes.BlockSize / 8);
                CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(input, 0, input.Length);
                cs.Close();
                return ms.ToArray();
            }
        }
        public byte[] Decrypt(byte[] input, string password, byte[] salt)
        {
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, salt);
            using (MemoryStream ms = new MemoryStream())
            using (Aes aes = new AesManaged())
            {

                aes.Key = pdb.GetBytes(aes.KeySize / 8);
                aes.IV = pdb.GetBytes(aes.BlockSize / 8);
                CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(input, 0, input.Length);
                cs.Close();
                return ms.ToArray();

            }
        }

        public static bool ZipDirectory(string directoryPath, string outputZipPath)
        {
            try
            {
                if (File.Exists(outputZipPath))
                {
                    File.Delete(outputZipPath);
                }

                ZipFile.CreateFromDirectory(directoryPath, outputZipPath);

                return true;
            }

            catch
            {
                return false;
            }
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "암호화하려는 파일을 선택하세요";
                ofd.InitialDirectory = @"C:\Download\";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    using(SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Title = "암호화된 파일을 저장할 경로를 입력하세요.";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {

                            try
                            {
                                File.WriteAllBytes(sfd.FileName
                                    , Encrypt(File.ReadAllBytes(ofd.FileName)
                                    , this.tbPassword.Text
                                    , this.SALT));

                                MessageBox.Show("저장되었습니다"
                                    , "성공"
                                    , MessageBoxButtons.OK
                                    , MessageBoxIcon.Information);

                                File.Delete(ofd.FileName);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message
                                    , "오류"
                                    , MessageBoxButtons.OK
                                    , MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "복호화하려는 파일을 선택하세요";
                ofd.InitialDirectory = @"C:\Download\";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Title = "복호화된 파일을 저장할 경로를 입력하세요";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {

                            try
                            {

                                File.WriteAllBytes(sfd.FileName
                                    , Decrypt(File.ReadAllBytes(ofd.FileName)
                                    , this.tbPassword.Text
                                    , this.SALT));

                                MessageBox.Show("저장되었습니다"
                                    , "성공"
                                    , MessageBoxButtons.OK
                                    , MessageBoxIcon.Information);

                                File.Delete(ofd.FileName);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message
                                    , "오류"
                                    , MessageBoxButtons.OK
                                    , MessageBoxIcon.Error);
                            }

                        }
                    }
                }
            }
        }


        private void metroPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void EncryptForm_Load(object sender, EventArgs e)
        {

        }

        private void lbKeyword_Click(object sender, EventArgs e)
        {

        }
    }
}
