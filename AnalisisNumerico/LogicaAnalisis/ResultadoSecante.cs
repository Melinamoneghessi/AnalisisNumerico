using System.Collections.Generic;

namespace LogicaAnalisis
{
    public class ResultadoSecante
    {
        public string Funcion { get; set; }
        public string Metodo { get; set; }
        public int IteracionesRealizadas { get; set; }
        public double Tolerancia { get; set; }
        public double XiInicial { get; set; }
        public double XdInicial { get; set; }
        public bool Converge { get; set; }
        public double Raiz { get; set; }
        public double Error { get; set; }
        public string Mensaje { get; set; }
        public List<IteracionSecante> Iteraciones { get; set; }

        public ResultadoSecante()
        {
            Metodo = "Secante";
            Mensaje = "";
            Iteraciones = new List<IteracionSecante>();
        }
    }
}
