using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System.IO;

namespace Duocare.Services;

public static class PdfService
{
    public static byte[] GenerateAgreementsPdf()
    {
        using var document = new PdfDocument();
        var page = document.Pages.Add();

        var fontTitle = new PdfStandardFont(PdfFontFamily.Helvetica, 18, PdfFontStyle.Bold);
        var fontHeader = new PdfStandardFont(PdfFontFamily.Helvetica, 13, PdfFontStyle.Bold);
        var fontBody = new PdfStandardFont(PdfFontFamily.Helvetica, 11);

        float y = 20;

        void Write(string text, PdfFont font)
        {
            page.Graphics.DrawString(text, font, PdfBrushes.Black, new Syncfusion.Drawing.PointF(20, y));
            y += font.MeasureString(text).Height + 8;

            // Salto de página automático
            if (y > page.GetClientSize().Height - 40)
            {
                page = document.Pages.Add();
                y = 20;
            }
        }

        // ============================
        // ACUERDO CUSTODIA MENOR
        // ============================

        Write("ACUERDO DE CUSTODIA Y RÉGIMEN DE VISITAS DEL MENOR", fontTitle);

        Write("I. IDENTIFICACIÓN DE LAS PARTES", fontHeader);
        Write("De una parte, D./Dña. ________________________________, y de otra, D./Dña. ________________________________, en calidad de progenitores del menor.", fontBody);
        Write("Fecha: ____ / ____ / ______", fontBody);
        Write("Lugar: ____________________________________________", fontBody);

        Write("II. OBJETO DEL ACUERDO", fontHeader);
        Write("El presente acuerdo tiene por objeto regular las condiciones de guarda, custodia, convivencia y régimen de visitas del menor, velando en todo momento por su interés superior.", fontBody);

        Write("III. RÉGIMEN DE CUSTODIA", fontHeader);
        Write("Se establece un régimen de custodia compartida, alternándose los progenitores en períodos semanales.", fontBody);
        Write("El cambio de custodia se realizará los viernes a las 18:00 horas en el domicilio acordado por ambas partes.", fontBody);

        Write("IV. VACACIONES Y DÍAS FESTIVOS", fontHeader);
        Write("Los periodos vacacionales se dividirán por mitades entre ambos progenitores.", fontBody);
        Write("En años pares, el progenitor/a 1 elegirá en primer lugar; en años impares, lo hará el progenitor/a 2.", fontBody);

        Write("V. COMUNICACIÓN Y DECISIONES", fontHeader);
        Write("Ambas partes se comprometen a mantener una comunicación fluida, respetuosa y orientada al bienestar del menor.", fontBody);
        Write("Las decisiones relevantes sobre educación, salud o residencia deberán adoptarse de mutuo acuerdo.", fontBody);

        Write("VI. RESOLUCIÓN DE CONFLICTOS", fontHeader);
        Write("En caso de discrepancia, las partes se comprometen a acudir previamente a un proceso de mediación familiar.", fontBody);

        Write("VII. FIRMA DE LAS PARTES", fontHeader);
        Write("Progenitor/a 1: ____________________________    Fecha: ____ / ____ / ______", fontBody);
        Write("Progenitor/a 2: ____________________________    Fecha: ____ / ____ / ______", fontBody);

        // Espacio entre acuerdos
        y += 20;

        // ============================
        // ACUERDO MASCOTA
        // ============================

        Write("ACUERDO DE CUSTODIA Y CUIDADO DE MASCOTA", fontTitle);

        Write("I. IDENTIFICACIÓN DE LAS PARTES", fontHeader);
        Write("D./Dña. ________________________________ y D./Dña. ________________________________, copropietarios de la mascota descrita a continuación.", fontBody);

        Write("II. IDENTIFICACIÓN DE LA MASCOTA", fontHeader);
        Write("Nombre: ________________________________", fontBody);
        Write("Especie/Raza: ___________________________", fontBody);
        Write("Edad: ____________", fontBody);

        Write("III. OBJETO DEL ACUERDO", fontHeader);
        Write("El presente acuerdo regula la convivencia, cuidado y responsabilidades sobre la mascota compartida.", fontBody);

        Write("IV. RÉGIMEN DE CUSTODIA", fontHeader);
        Write("La custodia de la mascota será compartida en periodos alternos acordados entre las partes.", fontBody);
        Write("Los intercambios se realizarán en el lugar y horario previamente pactados.", fontBody);

        Write("V. GASTOS Y RESPONSABILIDADES", fontHeader);
        Write("Ambas partes asumirán de forma equitativa los gastos ordinarios y extraordinarios derivados del cuidado del animal.", fontBody);

        Write("VI. BIENESTAR DEL ANIMAL", fontHeader);
        Write("Ambas partes se comprometen a garantizar el bienestar físico y emocional de la mascota, asegurando alimentación, atención veterinaria y condiciones adecuadas.", fontBody);

        Write("VII. RESOLUCIÓN DE CONFLICTOS", fontHeader);
        Write("Las partes intentarán resolver de forma amistosa cualquier desacuerdo, recurriendo a mediación si fuera necesario.", fontBody);

        Write("VIII. FIRMA DE LAS PARTES", fontHeader);
        Write("Parte 1: ____________________________    Fecha: ____ / ____ / ______", fontBody);
        Write("Parte 2: ____________________________    Fecha: ____ / ____ / ______", fontBody);

        using var stream = new MemoryStream();
        document.Save(stream);
        return stream.ToArray();
    }
}