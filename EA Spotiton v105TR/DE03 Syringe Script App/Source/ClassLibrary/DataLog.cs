using System;
using System.IO;
using System.Text;

namespace EA.PixyControl.ClassLibrary
{
    public class Datalog
    {
        private StreamWriter _writer;
        private FileInfo _file;
        private string _fullPathName;

        public Datalog(String filename)
        {
            OpenAsFile(filename, true);
            _fullPathName = filename;
        }

        public Datalog(String filename, bool append)
        {
            OpenAsFile(filename, append);
            _fullPathName = filename;
        }

        ~Datalog()
        {
            Close();
        }

        private void OpenAsFolder(string folder)
        {
            StringBuilder fullName = new StringBuilder();
            try
            {
                if (folder.IndexOf(":") < 0)
                {
                    fullName.Append(@"..\log\");
                }
                else
                {
                    fullName.Append(folder);
                }
                if (!Directory.Exists(fullName.ToString()))
                {
                    Directory.CreateDirectory(fullName.ToString());
                }
                fullName.Append(@"\");
                fullName.Append(folder);
                if (!Directory.Exists(fullName.ToString()))
                {
                    Directory.CreateDirectory(fullName.ToString());
                }

                fullName.AppendFormat(@"\{0}.log", DateTime.Now.Day.ToString("00"));
                _file = new FileInfo(fullName.ToString());
                if (_file.Exists)
                {
                    // change CreationTime to LastAccessTime, and then to LastWriteTime
                    if (_file.LastWriteTime.Month != DateTime.Now.Month)
                    {
                        _writer = _file.CreateText();
                    }
                    else
                    {
                        _writer = _file.AppendText();
                    }
                }
                else
                {
                    _writer = _file.CreateText();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void OpenAsFile(string filename, bool Append)
        {
            if (filename.IndexOf(":") < 0)
            {
                filename = @"..\log\" + filename;
            }
            try
            {
                _file = new FileInfo(filename);
                if (Append)
                {
                    if (_file.Exists)
                    {
                        _writer = _file.AppendText();
                    }
                    else
                    {
                        _writer = _file.CreateText();
                    }
                }
                else
                {
                    _writer = _file.CreateText();
                }
            }
            catch (Exception Ex)
            {
                System.Windows.Forms.MessageBox.Show("Error opening log file " + filename + "\n\n" + Ex.Message);
            }
        }

        public void WriteLine(string message)
        {
            try
            {
                if (_writer != null)
                {
                    _writer.WriteLine(message);
                }
            }
            catch (Exception)
            {
                Close();
            }
        }

        public void WriteLine(StringBuilder message)
        {
            this.WriteLine(message.ToString());
        }

        public void WriteLine(string format, params object[] arg)
        {
            try
            {
                if (_writer != null)
                {
                    _writer.WriteLine(format, arg);
                }
            }
            catch (Exception)
            {
                Close();
            }
        }

        public void WriteLine(string[] message)
        {
            try
            {
                foreach (string s in message)
                {
                    _writer.WriteLine(s);
                }
            }
            catch (Exception)
            {
                Close();
            }
        }

        public void Write(string message)
        {
            try
            {
                if (_writer != null)
                {
                    _writer.Write(message);
                }
            }
            catch (Exception)
            {
                Close();
            }
        }

        public void Flush()
        {
            try
            {
                if (_writer != null) _writer.Flush();
            }
            catch (Exception)
            {
                Close();
            }
        }

        public void Close()
        {
            try
            {
                if (_writer != null)
                {
                    _writer.Close();
                    _writer = null;
                }
            }
            catch (Exception) { }
        }

        public void Backup()
        {
            try
            {
                string filename = _file.FullName;
                int index = filename.LastIndexOf(".");
                string backupName = filename.Substring(0, index) + ".bck";

                FileInfo backupFile = new FileInfo(backupName);
                if (backupFile.Exists)
                {
                    backupFile.Delete();
                }
                _file.MoveTo(backupName);
                _file = new FileInfo(filename);
            }
            catch
            {
            }
        }

        public static string Timestamp()
        {
            DateTime time = DateTime.Now;
            StringBuilder message = new StringBuilder();
            message.AppendFormat("{0}/{1} {2}:{3}:{4}:{5} ",
                time.Month.ToString("00"),
                time.Day.ToString("00"),
                time.Hour.ToString("00"),
                time.Minute.ToString("00"),
                time.Second.ToString("00"),
                time.Millisecond.ToString("000"));
            return message.ToString();
        }
    }
}