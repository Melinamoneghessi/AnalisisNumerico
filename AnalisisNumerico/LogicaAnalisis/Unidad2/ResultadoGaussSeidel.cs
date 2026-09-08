using System.Collections.Generic;

namespace LogicaAnalisis.Unidad2
{
    public class ResultadoGaussSeidel
    {
        public string Metodo { get; set; }
        public int Dimension { get; set; }
        public double[] VectorResultado { get; set; }
        public bool Converge { get; set; }
        public string Mensaje { get; set; }
        public List<IteracionGaussSeidel> Iteraciones { get; set; }

        public ResultadoGaussSeidel()
        {
            Metodo = "Gauss-Seidel";
            Mensaje = "";
            Iteraciones = new List<IteracionGaussSeidel>();
        }
    }
}
