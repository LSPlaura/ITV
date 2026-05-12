using System.IO;
using System.Text;
using CSharpFunctionalExtensions;
using ITV.Error.Common;
using ITV.Error.Export;
using ITV.Models;
using SelectPdf;

namespace ITV.Service.Export;

public class ExportService : IExport<Cita>
{
    private static readonly string _fecha = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
    private string _file;
    private string _folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

    public ExportService(string file)
    {
        _file = file;
        
    }
    
    private string GenerateHtmlContent(Cita item)
    {
        return $@"
            <!DOCTYPE html>
            <html lang='es'>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f7f9; display: flex; justify-content: center; padding: 40px; }}
                    .card {{ background: white; width: 450px; border-radius: 12px; box-shadow: 0 10px 25px rgba(0,0,0,0.05); overflow: hidden; border: 1px solid #e1e8ed; }}
                    .header {{ background: #2c3e50; color: white; padding: 25px; text-align: center; }}
                    .header h2 {{ margin: 0; font-size: 1.2rem; letter-spacing: 1px; text-transform: uppercase; }}
                    .content {{ padding: 30px; }}
                    .section {{ margin-bottom: 20px; }}
                    .label {{ font-size: 0.75rem; color: #7f8c8d; text-transform: uppercase; font-weight: bold; margin-bottom: 5px; display: block; }}
                    .value {{ font-size: 1rem; color: #2c3e50; font-weight: 500; }}
                    .grid {{ display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }}
                    .footer {{ background: #fcfcfc; padding: 15px; text-align: center; border-top: 1px dashed #e1e8ed; color: #bdc3c7; font-size: 0.8rem; }}
                    .badge {{ background: #3498db; color: white; padding: 3px 10px; border-radius: 50px; font-size: 0.8rem; }}
                    .matricula {{ font-size: 1.5rem; font-weight: bold; color: #2c3e50; border: 2px solid #2c3e50; display: inline-block; padding: 2px 10px; border-radius: 4px; margin-top: 5px; }}
                </style>
            </head>
            <body>
                <div class='card'>
                    <div class='header'>
                        <h2>Comprobante de Cita ITV</h2>
                    </div>
                    <div class='content'>
                        <div class='section' style='text-align: center;'>
                            <span class='label'>Matrícula</span>
                            <div class='matricula'>{item.Matricula}</div>
                        </div>

                        <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'>

                        <div class='grid'>
                            <div class='section'>
                                <span class='label'>Vehículo</span>
                                <div class='value'>{item.Marca}</div>
                                <div class='value' style='color: #7f8c8d;'>{item.Modelo}</div>
                            </div>
                            <div class='section'>
                                <span class='label'>Motor / Cilindrada</span>
                                <div class='value'><span class='badge'>{item.Motor}</span></div>
                                <div class='value' style='margin-top:5px'>{item.Cilindrada} cc</div>
                            </div>
                        </div>

                        <div class='grid'>
                            <div class='section'>
                                <span class='label'>Fecha Inspección</span>
                                <div class='value'>{item.FechaInspeccion:dd/MM/yyyy}</div>
                            </div>
                            <div class='section'>
                                <span class='label'>DNI Propietario</span>
                                <div class='value'>{item.DniDueño}</div>
                            </div>
                        </div>

                        <div class='section'>
                            <span class='label'>Fecha Matriculación</span>
                            <div class='value'>{item.FechaMatriculacion:dd/MM/yyyy}</div>
                        </div>
                    </div>
                    <div class='footer'>
                        Generado el {DateTime.Now:dd/MM/yyyy HH:mm}
                    </div>
                </div>
            </body>
            </html>";
    }
    
        public Result<string, DomainError> ExportHtml(Cita item)
        {
            var path = Path.Combine(_folder, _file + $"_{_fecha}" + ".html");
            try
            {
                var html = GenerateHtmlContent(item);
                File.WriteAllText(path, html, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return Result.Failure<string, DomainError>(new ExportError.ExportHtmlError(ex.Message));
            }
            return Result.Success<string, DomainError>(path);
        }

        public Result<string, DomainError> ExportPdf(Cita item)
        {
            var path = Path.Combine(_folder, _file + $"_{_fecha}" + ".pdf");
            try
            {
                var html = GenerateHtmlContent(item);
                HtmlToPdf converter = new HtmlToPdf();
                var pdf = converter.ConvertHtmlString(html);
                pdf.Save(path);
                pdf.Close();
            }
            catch (Exception ex)
            {
                return Result.Failure<string, DomainError>(new ExportError.ExportPdfError(ex.Message));
            }
            return Result.Success<string, DomainError>(path);
        }
}