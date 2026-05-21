using System.IO;
using System.Text;
using CSharpFunctionalExtensions;
using ITV.Error.Common;
using ITV.Error.Export;
using ITV.Models;
using SelectPdf;

namespace ITV.Service.Export;

public class ReportGeneratorService : ReportGenerator<Cita>
{
    private static readonly string _fecha = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
    private string _folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
    
   private string GenerateHtmlContent(Cita item)
{
    return $@"
        <!DOCTYPE html>
        <html lang='es'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <style>
                * {{
                    margin: 0;
                    padding: 0;
                    box-sizing: border-box;
                }}

                body {{ 
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
                    background: white;
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    min-height: 100vh;
                    padding: 20px;
                }}

                .container {{
                    width: 100%;
                    max-width: 600px;
                    background: #FBFBFB;
                    border-radius: 8px;
                    border: solid 1px grey;
                    overflow: hidden;
                }}

                .header {{
                    background: #7D8A8A;
                    color: white;
                    padding: 40px 20px;
                    text-align: center;
                }}

                .header h1 {{
                    font-size: 24px;
                    font-weight: 800;
                    letter-spacing: 1px;
                    margin-bottom: 12px;
                }}

                .accent-line {{
                    width: 40px;
                    height: 3px;
                    background: #FF63C2;
                    margin: 0 auto;
                    border-radius: 2px;
                }}

                .content {{
                    padding: 40px;
                }}

                .field-label {{
                    font-size: 12px;
                    font-weight: 900;
                    color: #7D8A8A;
                    margin-bottom: 6px;
                    opacity: 0.8;
                    letter-spacing: 0.5px;
                    text-transform: uppercase;
                }}

                .field-value {{
                    font-size: 15px;
                    color: #1A1C1E;
                    margin-bottom: 20px;
                    padding: 5px 2px;
                }}

                .matricula-value {{
                    color: #FF63C2;
                    font-weight: bold;
                    font-size: 17px;
                }}

                .grid-2 {{
                    display: grid;
                    grid-template-columns: 1fr 1fr;
                    gap: 24px;
                    margin-bottom: 25px;
                }}

                .grid-2-full {{
                    grid-column: 1 / -1;
                }}

                .section-full {{
                    margin-bottom: 25px;
                }}

                .fecha-inspeccion {{
                    font-weight: 600;
                    color: #116E6B;
                }}

                .separator {{
                    height: 1px;
                    background: #E0E5E5;
                    margin: 10px 0 25px 0;
                }}

                .footer {{
                    background: #f0f0f0;
                    padding: 20px;
                    text-align: center;
                    border-top: 1px solid #E0E5E5;
                    color: #7D8A8A;
                    font-size: 0.85rem;
                    margin-top: 25px;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <!-- HEADER -->
                <div class='header'>
                    <h1>DETALLE DE LA CITA</h1>
                    <div class='accent-line'></div>
                </div>

                <!-- CONTENT -->
                <div class='content'>
                    <!-- MATRÍCULA -->
                    <div class='section-full'>
                        <div class='field-label'>Matrícula</div>
                        <div class='field-value matricula-value'>{item.Matricula}</div>
                    </div>

                    <!-- GRID 2 COLUMNAS: MARCA Y MODELO -->
                    <div class='grid-2'>
                        <div>
                            <div class='field-label'>Marca</div>
                            <div class='field-value'>{item.Marca}</div>
                        </div>
                        <div>
                            <div class='field-label'>Modelo</div>
                            <div class='field-value'>{item.Modelo}</div>
                        </div>
                    </div>

                    <!-- GRID 2 COLUMNAS: CILINDRADA Y MOTOR -->
                    <div class='grid-2'>
                        <div>
                            <div class='field-label'>Cilindrada (cc)</div>
                            <div class='field-value'>{item.Cilindrada}</div>
                        </div>
                        <div>
                            <div class='field-label'>Tipo de Motor</div>
                            <div class='field-value'>{item.Motor}</div>
                        </div>
                    </div>

                    <!-- DNI DEL DUEÑO -->
                    <div class='section-full'>
                        <div class='field-label'>DNI del Dueño</div>
                        <div class='field-value'>{item.DniDueño}</div>
                    </div>

                    <!-- GRID 2 COLUMNAS: FECHAS -->
                    <div class='grid-2'>
                        <div>
                            <div class='field-label'>Fecha de Matriculación</div>
                            <div class='field-value'>{item.FechaMatriculacion:dd/MM/yyyy}</div>
                        </div>
                        <div>
                            <div class='field-label'>Fecha de Inspección</div>
                            <div class='field-value fecha-inspeccion'>{item.FechaInspeccion:dd/MM/yyyy HH:mm}</div>
                        </div>
                    </div>

                    <!-- SEPARADOR -->
                    <div class='separator'></div>
                    <!-- FOOTER -->
                    <div class='footer'>
                        ✓ Generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}
                    </div>
                </div>
            </div>
        </body>
        </html>";
}
        public Result<string, DomainError> ExportHtml(Cita item)
        {
            var fechaFormato = item.FechaInspeccion.ToString("dd-MM-yyyy-HHmm");
            var file = $"{item.Matricula}_{fechaFormato}_{_fecha}.html";
            var path = Path.Combine(_folder, file);
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
            var fechaFormato = item.FechaInspeccion.ToString("dd-MM-yyyy-HHmm");
            var file = $"{item.Matricula}_{fechaFormato}_{_fecha}.pdf";
            var path = Path.Combine(_folder, file);
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