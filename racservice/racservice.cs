using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using Microsoft.Extensions.Configuration;
using racservice.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using FluentFTP;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace racservice
{
    public partial class racservice : ServiceBase
    {
        public EventLog eventLog1 { get; private set; }
        private int eventId = 1;
        private IConfiguration _configuration;
        private readonly DbHelper dbHelper;
        private ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        private Thread _thread;
        public racservice(IConfiguration configuration)
        {
            InitializeComponent();
            this._configuration = configuration;
            dbHelper = new DbHelper(_configuration);
            eventLog1 = new EventLog();
            if (!EventLog.SourceExists("RacService"))
            {
                EventLog.CreateEventSource("RacService", "MyLogRac");
            }
            eventLog1.Source = "RacService";
            eventLog1.Log = "MyLogRac";
        }

        

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("In OnStart.");
            base.OnStart(args);
            _thread = new Thread(Integration)
            {
                Name = "Rac Service",
                IsBackground = true
            };
            _thread.Start();
        }



        protected override void OnStop()
        {
            eventLog1.WriteEntry("In OnStop.");
        }

        public void Integration()
        {
            if (!Int32.TryParse(this._configuration["frequencyTime"], out int frequencyTime))
            {
                frequencyTime = 120000;
            }

            while (!_shutdownEvent.WaitOne(0))
            {
                this.Execute();
                Thread.Sleep(frequencyTime);
            }
        }

        private void Execute()
        {
            string[] files = Directory.GetFiles(_configuration["directory"]);

            string ftpHost = this._configuration["ftpHost"];
            string ftpUsername = this._configuration["ftpUsername"];
            string ftpEncryptedSecret = this._configuration["ftpEncryptedSecret"];
            string ftpUrl = this._configuration["pathFileDocument"];
            string ftpToUpload = string.Empty;
            string fileNameToUpload = string.Empty;
            foreach (string fileName in files)
            {
                var reader = new PdfReader(fileName);
                var extension = Path.GetExtension(fileName);
                fileNameToUpload = string.Concat(Guid.NewGuid().ToString(), extension);
                PdfDocument pdfDocument = new PdfDocument(reader);
                string[] words;
                string[] dates;

                try
                {
                    String firstPage = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(1));
                    words = firstPage.Split('\n');
                    var code = words[0].Substring(0, 5);
                    dates = words[3].Split(' ');
                    if (!DateTime.TryParse(dates[0], out DateTime startDate))
                    {

                    }
                    if (!DateTime.TryParse(dates[2], out DateTime endDate))
                    {

                    }
                    var registration = words[4];
                    eventLog1.WriteEntry(registration);
                    if (!Int32.TryParse(code, out int cod))
                    {
                        dbHelper.CreateLog(new Models.Log()
                        {
                            CreateDate = DateTime.Now,
                            Description = string.Concat("Formato de código da empresa inválido: ", cod),
                            Type = 1
                        });
                    }
                    var establishment = dbHelper.GetEstablishment(cod);
                    eventLog1.WriteEntry(establishment.Name);
                    if (establishment == null)
                    {
                        dbHelper.CreateLog(new Models.Log()
                        {
                            CreateDate = DateTime.Now,
                            Description = string.Concat("Empresa com o código: ", code, " não foi encontrada na plataforma."),
                            Type = 1
                        });
                    }
                    if (!Int32.TryParse(registration, out int reg))
                    {
                        dbHelper.CreateLog(new Models.Log()
                        {
                            CreateDate = DateTime.Now,
                            Description = string.Concat("Formate de matrícula inválido: ", registration),
                            Type = 1
                        });
                    }
                    var user = dbHelper.GetAspNetUsers(reg);
                    if (user == null)
                    {
                        dbHelper.CreateLog(new Models.Log()
                        {
                            CreateDate = DateTime.Now,
                            Description = string.Concat("Funcionário com a matrícula: ", registration, " não foi encontrado na plataforma."),
                            Type = 1
                        });
                    }
                    if (establishment != null &&
                        user != null &&
                        !string.IsNullOrEmpty(ftpHost) &&
                        !string.IsNullOrEmpty(ftpUsername) &&
                        !string.IsNullOrEmpty(ftpEncryptedSecret))
                    {
                        ftpToUpload = string.Concat(ftpUrl, fileNameToUpload);
                        using (var con = new FtpClient(ftpHost, ftpUsername, ftpEncryptedSecret))
                        {
                            con.Connect();
                            var status = con.UploadFile(
                                fileName,
                                ftpToUpload,
                                FtpRemoteExists.Overwrite,
                                true,
                                FtpVerify.Retry);

                            switch (status)
                            {
                                case FtpStatus.Success:
                                    eventLog1.WriteEntry(string.Concat("Upload com sucesso em: ", DateTime.Now.ToString()));
                                    con.Disconnect();
                                    var newDocument = new Models.Document();
                                    newDocument.EstablishmentId = establishment.Id;
                                    newDocument.ApplicationUserId = user.Id;
                                    newDocument.Description = "Contra-cheque";
                                    newDocument.StartDate = startDate;
                                    newDocument.EndDate = endDate;
                                    newDocument.CreateDate = DateTime.Now;
                                    newDocument.DocumentName = fileNameToUpload;
                                    dbHelper.CreateDocument(newDocument);
                                    eventLog1.WriteEntry(string.Concat("Documento criado com sucesso em: ", DateTime.Now.ToString()));
                                    reader.Close();
                                    var moving = string.Concat(_configuration["directoryToMove"], Path.GetFileName(fileName));
                                    File.Move(fileName, moving);
                                    sendEmailUploadDocument(user.Email, moving, "Contra-cheque");
                                    eventLog1.WriteEntry(string.Concat("E-mail enviado com sucesso em: ", DateTime.Now.ToString()));
                                    break;

                                case FtpStatus.Failed:
                                    dbHelper.CreateLog(new Models.Log()
                                    {
                                        CreateDate = DateTime.Now,
                                        Description = "Não foi possível efetuar o upload do arquivo. Verifique as configurações do FTP.",
                                        Type = 1
                                    });
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    dbHelper.CreateLog(new Models.Log()
                    {
                        CreateDate = DateTime.Now,
                        Description = string.Concat(ex.Message, " - ", ex.InnerException.ToString()),
                        Type = 1
                    });
                }
            }
        }

        public void sendEmailUploadDocument(string Email, string fileName, string description)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(_configuration["FromEmail"].ToString());
                mail.To.Add(Email);
                mail.Subject = string.Concat("RAC Contabilidade - Documento para download");
                Attachment data = new Attachment(fileName, MediaTypeNames.Application.Octet);
                ContentDisposition disposition = data.ContentDisposition;
                disposition.CreationDate = File.GetCreationTime(fileName);
                disposition.ModificationDate = File.GetLastWriteTime(fileName);
                disposition.ReadDate = File.GetLastAccessTime(fileName);
                mail.Attachments.Add(data);
                mail.Body = "<div style='padding-top: 15px;'>" + description + "</div>";
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient(_configuration["STMPEmail"].ToString(), Convert.ToInt32(_configuration["PortEmail"].ToString()));
                smtp.Credentials = new System.Net.NetworkCredential(_configuration["UserEmail"].ToString(), _configuration["PassEmail"].ToString());
                smtp.Send(mail);
            }
            catch (SmtpFailedRecipientException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (SmtpException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
