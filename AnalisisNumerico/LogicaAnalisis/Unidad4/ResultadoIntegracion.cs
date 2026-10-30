using System.Collections.Generic;

namespace LogicaAnalisis.Unidad4
{
    public class ResultadoIntegracion
    {
        public string Funcion { get; internal set; }
        public string SegundaFuncion { get; internal set; }
        public bool EsAreaEntreCurvas { get { return SegundaCurva != null; } }
        public string MetodoUtilizado { get; internal set; }
        public double ExtremoIzquierdo { get; internal set; }
        public double ExtremoDerecho { get; internal set; }
        public int Subintervalos { get; internal set; }
        public double Paso { get; internal set; }
        public bool PasoUniforme { get; internal set; }
        public bool UsaTabla { get; internal set; }
        public double Integral { get; internal set; }
        public IReadOnlyList<double[]> Curva { get; internal set; }
        public IReadOnlyList<double[]> SegundaCurva { get; internal set; }
        public IReadOnlyList<double[]> Aproximacion { get; internal set; }
        public IReadOnlyList<double[]> Nodos { get; internal set; }
    }
}
