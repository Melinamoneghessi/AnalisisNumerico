using System.Collections.Generic;
using System.Globalization;

namespace LogicaAnalisis.Unidad3
{
    public class ResultadoAjuste
    {
        public int Grado { get; internal set; }
        public double[] Coeficientes { get; internal set; }
        public string Funcion { get; internal set; }
        public double CorrelacionPorcentaje { get; internal set; }
        public double ToleranciaPorcentaje { get; internal set; }
        public List<PuntoAjuste> Puntos { get; internal set; }
        internal double[] CoeficientesNormalizados { get; set; }
        internal double CentroX { get; set; }
        internal double EscalaX { get; set; }

        public bool EsAceptable
        {
            get { return CorrelacionPorcentaje >= ToleranciaPorcentaje; }
        }

        public string Expresion
        {
            get
            {
                string variable = "((x-(" + FormatearNumero(CentroX) + "))/(" + FormatearNumero(EscalaX) + "))";
                string expresion = FormatearNumero(CoeficientesNormalizados[CoeficientesNormalizados.Length - 1]);
                for (int i = CoeficientesNormalizados.Length - 2; i >= 0; i--)
                {
                    expresion = "(" + expresion + "*" + variable + "+(" + FormatearNumero(CoeficientesNormalizados[i]) + "))";
                }
                return expresion;
            }
        }

        private string FormatearNumero(double valor)
        {
            string numero = valor.ToString("R", CultureInfo.InvariantCulture);
            int exponente = numero.IndexOf('E');
            return exponente < 0 ? numero : "(" + numero.Substring(0, exponente) + "*10^(" + numero.Substring(exponente + 1) + "))";
        }

        public double Evaluar(double x)
        {
            double normalizado = (x - CentroX) / EscalaX;
            double y = 0;
            for (int i = CoeficientesNormalizados.Length - 1; i >= 0; i--)
            {
                y = y * normalizado + CoeficientesNormalizados[i];
            }
            return y;
        }
    }
}
