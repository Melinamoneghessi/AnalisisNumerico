using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using LogicaAnalisis.Unidad2;

namespace LogicaAnalisis.Unidad3
{
    public class MetodoAjusteCurvas
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");

        public ResultadoAjuste Calcular(IList<PuntoAjuste> puntos, int grado, double toleranciaPorcentaje)
        {
            ValidarEntrada(puntos, grado, toleranciaPorcentaje);
            double minimo = puntos[0].X;
            double maximo = minimo;
            double escalaY = 1;
            foreach (PuntoAjuste punto in puntos)
            {
                minimo = Math.Min(minimo, punto.X);
                maximo = Math.Max(maximo, punto.X);
                escalaY = Math.Max(escalaY, Math.Abs(punto.Y));
            }

            double centro = minimo / 2 + maximo / 2;
            double escala = maximo / 2 - minimo / 2;
            if (escala <= 0 || !EsFinito(escala))
            {
                throw new ArgumentException("El rango de las coordenadas X no permite calcular el ajuste.");
            }

            // Normalizar X evita elevar coordenadas grandes a potencias de hasta 20.
            double[,] matriz = GenerarMatrizPolinomial(puntos, grado, centro, escala, escalaY);
            double[] normalizados;
            try
            {
                normalizados = new MetodoGaussJordan().Calcular(matriz).VectorResultado;
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("No se puede obtener un ajuste unico con estos puntos y este grado. Revise las coordenadas X o reduzca el grado.");
            }
            for (int i = 0; i < normalizados.Length; i++)
            {
                normalizados[i] *= escalaY;
            }

            double[] coeficientes = ConvertirCoeficientes(normalizados, centro, escala);
            return CrearResultado(puntos, grado, toleranciaPorcentaje, coeficientes, normalizados, centro, escala);
        }

        public ResultadoAjuste EvaluarRectaModificada(IList<PuntoAjuste> puntos, string funcion, double toleranciaPorcentaje)
        {
            ValidarEntrada(puntos, 1, toleranciaPorcentaje);
            double[] coeficientes = LeerRecta(funcion);
            return CrearResultado(puntos, 1, toleranciaPorcentaje, coeficientes, coeficientes, 0, 1);
        }

        private void ValidarEntrada(IList<PuntoAjuste> puntos, int grado, double tolerancia)
        {
            if (grado < 1 || grado > 10)
            {
                throw new ArgumentException("El grado debe estar entre 1 y 10.");
            }
            if (!EsFinito(tolerancia) || tolerancia < 0 || tolerancia > 100)
            {
                throw new ArgumentException("La tolerancia debe estar entre 0 y 100%.");
            }
            if (puntos == null || puntos.Count < grado + 1)
            {
                throw new ArgumentException("El grado " + grado + " requiere al menos " + (grado + 1) + " puntos.");
            }
            HashSet<double> coordenadasX = new HashSet<double>();
            foreach (PuntoAjuste punto in puntos)
            {
                if (punto == null || !EsFinito(punto.X) || !EsFinito(punto.Y))
                {
                    throw new ArgumentException("Las coordenadas deben ser numeros finitos.");
                }
                coordenadasX.Add(punto.X);
            }
            if (coordenadasX.Count < grado + 1)
            {
                throw new ArgumentException("Se necesitan al menos " + (grado + 1) + " coordenadas X distintas para este grado.");
            }
        }

        private double[,] GenerarMatrizPolinomial(IList<PuntoAjuste> puntos, int grado, double centro, double escala, double escalaY)
        {
            double[] sumas = new double[grado * 2 + 1];
            double[] terminos = new double[grado + 1];
            foreach (PuntoAjuste punto in puntos)
            {
                double x = (punto.X - centro) / escala;
                double potencia = 1;
                for (int exponente = 0; exponente < sumas.Length; exponente++)
                {
                    sumas[exponente] += potencia;
                    if (exponente <= grado)
                    {
                        terminos[exponente] += potencia * (punto.Y / escalaY);
                    }
                    potencia *= x;
                }
            }
            double[,] matriz = new double[grado + 1, grado + 2];
            for (int fila = 0; fila <= grado; fila++)
            {
                for (int columna = 0; columna <= grado; columna++)
                {
                    matriz[fila, columna] = sumas[fila + columna];
                }
                matriz[fila, grado + 1] = terminos[fila];
            }
            return matriz;
        }

        private double[] ConvertirCoeficientes(double[] normalizados, double centro, double escala)
        {
            double[] coeficientes = new double[normalizados.Length];
            double[] potencia = new double[normalizados.Length];
            potencia[0] = 1;
            for (int grado = 0; grado < normalizados.Length; grado++)
            {
                for (int termino = 0; termino <= grado; termino++)
                {
                    coeficientes[termino] += normalizados[grado] * potencia[termino];
                }
                if (grado == normalizados.Length - 1)
                {
                    break;
                }
                for (int termino = grado + 1; termino >= 0; termino--)
                {
                    potencia[termino] = (termino == 0 ? 0 : potencia[termino - 1] / escala)
                        - potencia[termino] * (centro / escala);
                }
            }
            foreach (double coeficiente in coeficientes)
            {
                if (!EsFinito(coeficiente))
                {
                    throw new ArgumentException("Las coordenadas producen coeficientes fuera del rango numerico permitido.");
                }
            }
            return coeficientes;
        }

        private ResultadoAjuste CrearResultado(IList<PuntoAjuste> puntos, int grado, double tolerancia,
            double[] coeficientes, double[] normalizados, double centro, double escala)
        {
            ResultadoAjuste resultado = new ResultadoAjuste
            {
                Grado = grado,
                Coeficientes = coeficientes,
                CoeficientesNormalizados = normalizados,
                CentroX = centro,
                EscalaX = escala,
                Funcion = FormatearFuncion(coeficientes),
                ToleranciaPorcentaje = tolerancia,
                Puntos = new List<PuntoAjuste>()
            };
            foreach (PuntoAjuste punto in puntos)
            {
                resultado.Puntos.Add(new PuntoAjuste(punto.X, punto.Y));
            }
            resultado.CorrelacionPorcentaje = CalcularCorrelacion(resultado);
            return resultado;
        }

        private double CalcularCorrelacion(ResultadoAjuste resultado)
        {
            double escalaY = 0;
            bool constante = true;
            foreach (PuntoAjuste punto in resultado.Puntos)
            {
                escalaY = Math.Max(escalaY, Math.Abs(punto.Y));
                constante = constante && punto.Y == resultado.Puntos[0].Y;
            }
            if (escalaY == 0)
            {
                escalaY = 1;
            }
            double media = 0;
            foreach (PuntoAjuste punto in resultado.Puntos)
            {
                media += punto.Y / escalaY / resultado.Puntos.Count;
            }
            double st = 0;
            double sr = 0;
            foreach (PuntoAjuste punto in resultado.Puntos)
            {
                double estimado = resultado.Evaluar(punto.X);
                if (!EsFinito(estimado))
                {
                    throw new ArgumentException("La funcion produce valores fuera del rango numerico permitido.");
                }
                double desviacion = punto.Y / escalaY - media;
                double residuo = punto.Y / escalaY - estimado / escalaY;
                st += desviacion * desviacion;
                sr += residuo * residuo;
            }
            if (constante || st == 0)
            {
                return sr <= 1e-24 * resultado.Puntos.Count ? 100 : 0;
            }
            return 100 * Math.Sqrt(Math.Max(0, Math.Min(1, 1 - sr / st)));
        }

        private double[] LeerRecta(string funcion)
        {
            string texto = Regex.Replace(funcion ?? "", @"\s+", "").ToLowerInvariant();
            if (texto.StartsWith("y="))
            {
                texto = texto.Substring(2);
            }
            const string numero = @"(?:\d+(?:[.,]\d*)?|[.,]\d+)(?:e[+-]?\d+)?";
            string patron = @"(?<signo>[+-]?)(?:(?<pendiente>" + numero + @")\*?x|(?<variable>x)|(?<constante>" + numero + "))";
            MatchCollection terminos = Regex.Matches(texto, patron);
            double pendiente = 0;
            double intercepto = 0;
            int posicion = 0;
            foreach (Match termino in terminos)
            {
                if (termino.Index != posicion || (posicion > 0 && termino.Groups["signo"].Value == ""))
                {
                    throw new ArgumentException("Ingrese una recta valida, por ejemplo: y = 2x - 3.");
                }
                double signo = termino.Groups["signo"].Value == "-" ? -1 : 1;
                if (termino.Groups["variable"].Success)
                {
                    pendiente += signo;
                }
                else if (termino.Groups["pendiente"].Success)
                {
                    pendiente += signo * LeerNumero(termino.Groups["pendiente"].Value);
                }
                else
                {
                    intercepto += signo * LeerNumero(termino.Groups["constante"].Value);
                }
                posicion += termino.Length;
            }
            if (posicion == 0 || posicion != texto.Length || !EsFinito(pendiente) || !EsFinito(intercepto))
            {
                throw new ArgumentException("Ingrese una recta valida, por ejemplo: y = 2x - 3.");
            }
            return new[] { intercepto, pendiente };
        }

        private double LeerNumero(string texto)
        {
            double valor;
            if (!double.TryParse(texto.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out valor) || !EsFinito(valor))
            {
                throw new ArgumentException("Los coeficientes de la recta deben ser numeros finitos.");
            }
            return valor;
        }

        private string FormatearFuncion(double[] coeficientes)
        {
            StringBuilder funcion = new StringBuilder("y = ");
            bool primerTermino = true;
            for (int grado = coeficientes.Length - 1; grado >= 0; grado--)
            {
                double valor = coeficientes[grado];
                if (valor == 0)
                {
                    continue;
                }
                funcion.Append(primerTermino ? (valor < 0 ? "-" : "") : (valor < 0 ? " - " : " + "));
                funcion.Append(Math.Abs(valor).ToString("G10", Cultura));
                if (grado > 0)
                {
                    funcion.Append("x");
                    if (grado > 1)
                    {
                        funcion.Append("^" + grado);
                    }
                }
                primerTermino = false;
            }
            if (primerTermino)
            {
                funcion.Append("0");
            }
            return funcion.ToString();
        }

        private bool EsFinito(double valor)
        {
            return !double.IsNaN(valor) && !double.IsInfinity(valor);
        }
    }
}
