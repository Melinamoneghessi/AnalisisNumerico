using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Calculus;

namespace LogicaAnalisis.Unidad4
{
    public class IntegradorNumerico
    {
        public const int MaximoSubintervalos = 10000;

        public ResultadoIntegracion Calcular(string funcion, double xi, double xd, int n, MetodoIntegracion metodo)
        {
            Calculo evaluador = CrearEvaluador(funcion);
            return CalcularFuncion(funcion.Trim().Replace(',', '.'), x => Evaluar(evaluador, x), xi, xd, n, metodo);
        }

        public ResultadoIntegracion CalcularEntreCurvas(string funcion, string segundaFuncion, double xi, double xd,
            int n, MetodoIntegracion metodo)
        {
            Calculo primera = CrearEvaluador(funcion), segunda = CrearEvaluador(segundaFuncion);
            ResultadoIntegracion resultado = CalcularFuncion(funcion.Trim(), x =>
            {
                double distancia = Math.Abs(Evaluar(primera, x) - Evaluar(segunda, x));
                if (!EsFinito(distancia)) throw new ArgumentException("La separacion entre las funciones excede el rango numerico permitido.");
                return distancia;
            }, xi, xd, n, metodo);
            List<double[]> curva = new List<double[]>(), segundaCurva = new List<double[]>();
            foreach (double[] punto in resultado.Curva)
            {
                curva.Add(new[] { punto[0], Evaluar(primera, punto[0]) });
                segundaCurva.Add(new[] { punto[0], Evaluar(segunda, punto[0]) });
            }
            resultado.Curva = curva.AsReadOnly();
            resultado.SegundaCurva = segundaCurva.AsReadOnly();
            resultado.SegundaFuncion = segundaFuncion.Trim();
            return resultado;
        }

        private static Calculo CrearEvaluador(string funcion)
        {
            if (string.IsNullOrWhiteSpace(funcion)) throw new ArgumentException("Ingrese una funcion.");
            funcion = funcion.Trim().Replace(',', '.');
            string expresion = Regex.Replace(funcion.ToLowerInvariant(), @"\bln\b", "log");
            expresion = Regex.Replace(expresion, @"\bsqrt\b", "sqr");
            // Calculus llama log al logaritmo natural y no reconoce e ni pi como constantes.
            expresion = Regex.Replace(expresion, @"\be\b", "(" + Math.E.ToString("R", CultureInfo.InvariantCulture) + ")");
            expresion = Regex.Replace(expresion, @"\bpi\b", "(" + Math.PI.ToString("R", CultureInfo.InvariantCulture) + ")");
            if (Regex.IsMatch(expresion, @"\b[a-wyz]\b"))
                throw new ArgumentException("La funcion solo puede depender de la variable x.");
            Calculo evaluador = new Calculo();
            try
            {
                if (!evaluador.Sintaxis(expresion, 'x')) throw new ArgumentException("La funcion tiene una sintaxis invalida.");
            }
            catch (Exception ex)
            {
                throw new ArgumentException("La funcion tiene una sintaxis invalida.", ex);
            }
            return evaluador;
        }

        private static ResultadoIntegracion CalcularFuncion(string funcion, Func<double, double> evaluar,
            double xi, double xd, int n, MetodoIntegracion metodo)
        {
            if (!Enum.IsDefined(typeof(MetodoIntegracion), metodo))
                throw new ArgumentException("Seleccione un metodo valido.");
            if (!EsFinito(xi) || !EsFinito(xd) || xi >= xd)
                throw new ArgumentException("El extremo izquierdo debe ser menor que el derecho; ambos deben ser finitos.");
            if (metodo == MetodoIntegracion.TrapeciosSimple) n = 1;
            if (metodo == MetodoIntegracion.SimpsonUnTercioSimple) n = 2;
            if (metodo == MetodoIntegracion.SimpsonTresOctavos) n = 3;
            int minimo = metodo == MetodoIntegracion.SimpsonUnTercioMultiple ? 2 : 1;
            if (n < minimo || n > MaximoSubintervalos)
                throw new ArgumentException("Este metodo requiere entre " + minimo + " y " + MaximoSubintervalos + " subintervalos.");
            double h = (xd - xi) / n;
            if (!EsFinito(h) || h <= 0 || xi + h == xi || xd - h == xd)
                throw new ArgumentException("El intervalo y la cantidad de subintervalos exceden la precision numerica disponible.");

            double[] xs = new double[n + 1];
            double[] ys = new double[n + 1];
            for (int i = 0; i <= n; i++)
            {
                xs[i] = i == n ? xd : xi + h * i;
                if (i > 0 && xs[i] <= xs[i - 1]) throw new ArgumentException("Los nodos no se pueden distinguir con la precision disponible.");
                ys[i] = evaluar(xs[i]);
                if (i < n) evaluar(xi + h * (i + 0.5));
            }

            List<double[]> curva = new List<double[]>();
            for (int i = 0; i <= 400; i++)
            {
                double x = i == 400 ? xd : xi + (xd - xi) * (i / 400.0);
                curva.Add(new[] { x, evaluar(x) });
            }
            return IntegrarNodos(funcion, xs, ys, metodo, curva, false, true);
        }

        public ResultadoIntegracion CalcularTabla(IList<PuntoIntegracion> puntos, MetodoIntegracion metodo)
        {
            if (!Enum.IsDefined(typeof(MetodoIntegracion), metodo))
                throw new ArgumentException("Seleccione un metodo valido.");
            if (puntos == null || puntos.Count < 2 || puntos.Count > MaximoSubintervalos + 1)
                throw new ArgumentException("Ingrese entre 2 y " + (MaximoSubintervalos + 1) + " puntos en la tabla.");

            int n = puntos.Count - 1;
            int requeridos = metodo == MetodoIntegracion.TrapeciosSimple ? 2
                : metodo == MetodoIntegracion.SimpsonUnTercioSimple ? 3
                : metodo == MetodoIntegracion.SimpsonTresOctavos ? 4 : 0;
            if (requeridos != 0 && puntos.Count != requeridos)
                throw new ArgumentException("El metodo seleccionado requiere exactamente " + requeridos + " puntos en la tabla.");
            if (metodo == MetodoIntegracion.SimpsonUnTercioMultiple && n < 2)
                throw new ArgumentException("Simpson 1/3 multiple requiere al menos 3 puntos.");

            double[] xs = new double[puntos.Count], ys = new double[puntos.Count];
            List<double[]> curva = new List<double[]>();
            for (int i = 0; i < puntos.Count; i++)
            {
                PuntoIntegracion punto = puntos[i];
                if (punto == null || !EsFinito(punto.X) || !EsFinito(punto.Y))
                    throw new ArgumentException("X y f(x) deben ser numeros finitos en la fila " + (i + 1) + ".");
                xs[i] = punto.X;
                ys[i] = punto.Y;
                if (i > 0 && (xs[i] <= xs[i - 1] || !EsFinito(xs[i] - xs[i - 1])))
                    throw new ArgumentException("Las X deben estar en orden creciente, sin repetirse. Revise la fila " + (i + 1) + ".");
                curva.Add(new[] { xs[i], ys[i] });
            }

            double h = (xs[n] - xs[0]) / n;
            if (!EsFinito(h) || h <= 0)
                throw new ArgumentException("El intervalo de la tabla excede la precision numerica disponible.");
            bool uniforme = true;
            for (int i = 1; i <= n; i++)
                if (Math.Abs((xs[i] - xs[i - 1]) / h - 1) > 1e-6) uniforme = false;
            if (!uniforme && metodo != MetodoIntegracion.TrapeciosMultiple)
                throw new ArgumentException("Los metodos de Simpson requieren la misma separacion entre los valores de X.");

            return IntegrarNodos(null, xs, ys, metodo, curva, true, uniforme);
        }

        private static ResultadoIntegracion IntegrarNodos(string funcion, double[] xs, double[] ys,
            MetodoIntegracion metodo, List<double[]> curva, bool usaTabla, bool pasoUniforme)
        {
            int n = xs.Length - 1;
            double xi = xs[0], xd = xs[n], h = (xd - xi) / n;
            List<double[]> nodos = new List<double[]>();
            for (int i = 0; i <= n; i++) nodos.Add(new[] { xs[i], ys[i] });
            List<Tramo> tramos = new List<Tramo>();
            string nombre;
            if (metodo == MetodoIntegracion.TrapeciosSimple || metodo == MetodoIntegracion.TrapeciosMultiple)
            {
                for (int i = 0; i < n; i++) tramos.Add(new Tramo(i, 1));
                nombre = metodo == MetodoIntegracion.TrapeciosSimple ? "Trapecios simple" : "Trapecios multiple";
            }
            else if (metodo == MetodoIntegracion.SimpsonTresOctavos)
            {
                tramos.Add(new Tramo(0, 3));
                nombre = "Simpson 3/8";
            }
            else
            {
                int ultimoPar = n % 2 == 0 ? n : n - 3;
                for (int i = 0; i < ultimoPar; i += 2) tramos.Add(new Tramo(i, 2));
                if (ultimoPar != n) tramos.Add(new Tramo(ultimoPar, 3));
                nombre = metodo == MetodoIntegracion.SimpsonUnTercioSimple ? "Simpson 1/3 simple"
                    : n == 3 ? "Simpson 3/8 (N = 3)"
                    : n % 2 == 0 ? "Simpson 1/3 multiple"
                    : "Simpson combinado: 1/3 (" + (n - 3) + ") + 3/8 (3)";
            }

            // Sumar contribuciones ya ponderadas reduce el desborde y el error acumulado.
            double integral = 0, compensacion = 0;
            foreach (Tramo tramo in tramos)
            {
                double paso = (xs[tramo.Inicio + tramo.Grado] - xs[tramo.Inicio]) / tramo.Grado;
                double[] pesos = tramo.Grado == 1 ? new[] { 0.5, 0.5 }
                    : tramo.Grado == 2 ? new[] { 1.0 / 3, 4.0 / 3, 1.0 / 3 }
                    : new[] { 3.0 / 8, 9.0 / 8, 9.0 / 8, 3.0 / 8 };
                for (int j = 0; j <= tramo.Grado; j++)
                {
                    double aporte = (ys[tramo.Inicio + j] * paso) * pesos[j] - compensacion;
                    double siguiente = integral + aporte;
                    compensacion = (siguiente - integral) - aporte;
                    integral = siguiente;
                }
            }
            if (!EsFinito(integral)) throw new ArgumentException("La integral excede el rango numerico permitido.");

            List<double[]> aproximacion = new List<double[]>();
            // En tablas se conserva cada nodo; para muchas filas se reduce el muestreo entre nodos.
            if (tramos.Count <= 100 || usaTabla)
            {
                foreach (Tramo tramo in tramos)
                {
                    int muestras = tramo.Grado == 1 ? 1 : tramos.Count > 100 ? tramo.Grado : 12;
                    for (int j = 0; j < muestras; j++)
                        aproximacion.Add(Muestrear(tramo, j * tramo.Grado / (double)muestras, xs, ys));
                }
            }
            else
            {
                int indice = 0;
                for (int i = 0; i < 600; i++)
                {
                    double posicion = n * (i / 600.0);
                    while (indice < tramos.Count - 1 && posicion >= tramos[indice].Inicio + tramos[indice].Grado) indice++;
                    aproximacion.Add(Muestrear(tramos[indice], posicion - tramos[indice].Inicio, xs, ys));
                }
            }
            aproximacion.Add(new[] { xd, ys[n] });
            return new ResultadoIntegracion
            {
                Funcion = funcion, ExtremoIzquierdo = xi, ExtremoDerecho = xd, Subintervalos = n, Paso = h,
                UsaTabla = usaTabla, PasoUniforme = pasoUniforme,
                Integral = integral, MetodoUtilizado = nombre, Curva = curva.AsReadOnly(),
                Aproximacion = aproximacion.AsReadOnly(), Nodos = nodos.AsReadOnly()
            };
        }

        private static double[] Muestrear(Tramo tramo, double t, double[] xs, double[] ys)
        {
            double h = (xs[tramo.Inicio + tramo.Grado] - xs[tramo.Inicio]) / tramo.Grado;
            double y = 0;
            for (int j = 0; j <= tramo.Grado; j++)
            {
                double baseLagrange = 1;
                for (int k = 0; k <= tramo.Grado; k++) if (j != k) baseLagrange *= (t - k) / (j - k);
                y += ys[tramo.Inicio + j] * baseLagrange;
            }
            if (!EsFinito(y)) throw new ArgumentException("La aproximacion excede el rango numerico permitido.");
            return new[] { xs[tramo.Inicio] + h * t, y };
        }

        private static double Evaluar(Calculo evaluador, double x)
        {
            double valor;
            try { valor = evaluador.EvaluaFx(x); }
            catch (Exception ex) { throw new ArgumentException("No se pudo evaluar la funcion en x = " + x.ToString("G8", CultureInfo.InvariantCulture) + ".", ex); }
            if (!EsFinito(valor)) throw new ArgumentException("La funcion no esta definida o no es finita en x = " + x.ToString("G8", CultureInfo.InvariantCulture) + ".");
            return valor;
        }

        private static bool EsFinito(double valor) { return !double.IsNaN(valor) && !double.IsInfinity(valor); }

        private sealed class Tramo
        {
            public int Inicio { get; private set; }
            public int Grado { get; private set; }
            public Tramo(int inicio, int grado) { Inicio = inicio; Grado = grado; }
        }
    }
}
