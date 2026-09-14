using System.Collections.Generic;

namespace LogicaAnalisis.Unidad2
{
    public class ResultadoGaussJordan
    {
        public string Metodo { get; set; }
        public int Dimension { get; set; }
        public double[,] MatrizFinal { get; set; }
        public double[] VectorResultado { get; set; }
        public double[] VectorResultadoModificado { get; set; }
        public double NumeroCondicion { get; set; }
        public string Condicionamiento { get; set; }
        public string Mensaje { get; set; }
        public List<IteracionGaussJordan> Iteraciones { get; set; }

        public ResultadoGaussJordan()
        {
            Metodo = "Gauss-Jordan";
            Mensaje = "";
            Condicionamiento = "";
            Iteraciones = new List<IteracionGaussJordan>();
        }
    }
}
