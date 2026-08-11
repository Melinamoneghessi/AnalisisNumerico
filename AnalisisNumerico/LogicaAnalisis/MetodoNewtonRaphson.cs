using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Calculus;

namespace LogicaAnalisis
{
    public class MetodoNewtonRaphson
    {
        public ResultadoNewtonRaphson Calcular(
            string funcion,
            double x0,
            double tolerancia,
            int iteracionesMaximas)
        {
            if (string.IsNullOrWhiteSpace(funcion))
            {
                throw new ArgumentException(
                    "Debe ingresar una funcion."
                );
            }

            if (tolerancia <= 0)
            {
                throw new ArgumentException(
                    "La tolerancia debe ser mayor a cero."
                );
            }

            if (iteracionesMaximas <= 0)
            {
                throw new ArgumentException(
                    "Las iteraciones maximas deben ser mayores a cero."
                );
            }

            funcion = funcion.Trim().Replace(',', '.');

            Calculo analizador = new Calculo();

            if (!analizador.Sintaxis(funcion, 'x'))
            {
                throw new ArgumentException(
                    "La funcion ingresada tiene una sintaxis invalida."
                );
            }

            ResultadoNewtonRaphson resultado =
                new ResultadoNewtonRaphson
                {
                    Funcion = funcion,
                    Tolerancia = tolerancia,
                    X0Inicial = x0
                };

            double xn = x0;

            // Primero verificamos si X0 ya es raiz
            double fxInicial = analizador.EvaluaFx(xn);

            if (double.IsNaN(fxInicial) ||
                double.IsInfinity(fxInicial))
            {
                throw new ArgumentException(
                    "La funcion no se puede evaluar en X0."
                );
            }

            if (Math.Abs(fxInicial) < tolerancia)
            {
                resultado.Converge = true;
                resultado.Raiz = xn;
                resultado.Error = 0;
                resultado.IteracionesRealizadas = 0;
                resultado.Mensaje =
                    "El valor inicial ya es una raiz.";

                return resultado;
            }

            double xrAnterior = 0;
            double error = double.MaxValue;

            for (int i = 1;
                 i <= iteracionesMaximas;
                 i++)
            {
                double fxn =
                    analizador.EvaluaFx(xn);

                if (double.IsNaN(fxn) ||
                    double.IsInfinity(fxn))
                {
                    resultado.Converge = false;
                    resultado.Mensaje =
                        "El metodo diverge. La funcion no esta definida.";

                    break;
                }

                // Derivada usando Calculus.dll
                double derivada =
                    analizador.Dx(xn);

                if (double.IsNaN(derivada) ||
                    double.IsInfinity(derivada) ||
                    Math.Abs(derivada) < tolerancia)
                {
                    resultado.Converge = false;
                    resultado.Mensaje =
                        "El metodo diverge. La derivada es cero o muy pequena.";

                    break;
                }

                // Formula de Newton-Raphson
                double xr =
                    xn - (fxn / derivada);

                if (double.IsNaN(xr) ||
                    double.IsInfinity(xr))
                {
                    resultado.Converge = false;
                    resultado.Mensaje =
                        "El metodo diverge. No encuentra raiz.";

                    break;
                }

                double fxr =
                    analizador.EvaluaFx(xr);

                if (double.IsNaN(fxr) ||
                    double.IsInfinity(fxr))
                {
                    resultado.Converge = false;
                    resultado.Mensaje =
                        "El metodo diverge. La funcion no esta definida.";

                    break;
                }

                // Error relativo, como indica el PDF
                if (i == 1)
                {
                    error = Math.Abs(
                        (xr - xn) / xr
                    );
                }
                else
                {
                    error = Math.Abs(
                        (xr - xrAnterior) / xr
                    );
                }

                resultado.Iteraciones.Add(
                    new IteracionNewtonRaphson
                    {
                        Iteracion = i,
                        Xn = xn,
                        Fxn = fxn,
                        Derivada = derivada,
                        Xsiguiente = xr,
                        Error = error
                    }
                );

                // Condicion de corte del algoritmo
                if (Math.Abs(fxr) < tolerancia ||
                    error < tolerancia)
                {
                    resultado.Converge = true;
                    resultado.Raiz = xr;
                    resultado.Error = error;
                    resultado.IteracionesRealizadas = i;
                    resultado.Mensaje =
                        "Raiz aproximada encontrada.";

                    return resultado;
                }

                xrAnterior = xr;
                xn = xr;
            }

            resultado.IteracionesRealizadas =
                resultado.Iteraciones.Count;

            if (resultado.Iteraciones.Count > 0)
            {
                IteracionNewtonRaphson ultima =
                    resultado.Iteraciones[
                        resultado.Iteraciones.Count - 1
                    ];

                resultado.Raiz =
                    ultima.Xsiguiente;

                resultado.Error =
                    ultima.Error;
            }
            else
            {
                resultado.Raiz = xn;
                resultado.Error = 0;
            }

            if (string.IsNullOrWhiteSpace(
                resultado.Mensaje))
            {
                resultado.Mensaje =
                    "Se alcanzo el maximo de iteraciones.";
            }

            return resultado;
        }
    }
}
