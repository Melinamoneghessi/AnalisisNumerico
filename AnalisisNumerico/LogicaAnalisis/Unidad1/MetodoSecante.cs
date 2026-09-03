using System;
using Calculus;

namespace LogicaAnalisis
{
    public class MetodoSecante
    {
        public ResultadoSecante Calcular(
            string funcion,
            double xi,
            double xd,
            double tolerancia,
            int iteracionesMaximas)
        {
            if (string.IsNullOrWhiteSpace(funcion))
            {
                throw new ArgumentException("Debe ingresar una funcion.");
            }

            if (tolerancia <= 0)
            {
                throw new ArgumentException("La tolerancia debe ser mayor a cero.");
            }

            if (iteracionesMaximas <= 0)
            {
                throw new ArgumentException("Las iteraciones maximas deben ser mayores a cero.");
            }

            if (xi == xd)
            {
                throw new ArgumentException("Xi y Xd deben ser valores distintos.");
            }

            funcion = funcion.Trim().Replace(',', '.');

            Calculo analizador = new Calculo();
            if (!analizador.Sintaxis(funcion, 'x'))
            {
                throw new ArgumentException("La funcion ingresada tiene una sintaxis invalida.");
            }

            ResultadoSecante resultado = new ResultadoSecante
            {
                Funcion = funcion,
                Tolerancia = tolerancia,
                XiInicial = xi,
                XdInicial = xd
            };

            double fxi = Evaluar(analizador, xi);
            double fxd = Evaluar(analizador, xd);

            if (Math.Abs(fxi) <= tolerancia)
            {
                resultado.Converge = true;
                resultado.Raiz = xi;
                resultado.Error = 0;
                resultado.Mensaje = "La raiz esta en Xi.";
                return resultado;
            }

            if (Math.Abs(fxd) <= tolerancia)
            {
                resultado.Converge = true;
                resultado.Raiz = xd;
                resultado.Error = 0;
                resultado.Mensaje = "La raiz esta en Xd.";
                return resultado;
            }

            for (int i = 1; i <= iteracionesMaximas; i++)
            {
                double denominador = fxd - fxi;

                if (Math.Abs(denominador) < tolerancia)
                {
                    resultado.Converge = false;
                    resultado.Mensaje = "El metodo diverge. El denominador es cero o muy pequeno.";
                    break;
                }

                double xr = xd - fxd * (xd - xi) / denominador;

                if (double.IsNaN(xr) || double.IsInfinity(xr))
                {
                    resultado.Converge = false;
                    resultado.Mensaje = "El metodo diverge. No encuentra raiz.";
                    break;
                }

                double fxr = Evaluar(analizador, xr);
                double error = CalcularError(xr, xd);

                resultado.Iteraciones.Add(new IteracionSecante
                {
                    Iteracion = i,
                    Xi = xi,
                    Xd = xd,
                    Fxi = fxi,
                    Fxd = fxd,
                    Xr = xr,
                    Fxr = fxr,
                    Error = error
                });

                if (Math.Abs(fxr) <= tolerancia || error <= tolerancia)
                {
                    resultado.Converge = true;
                    resultado.Raiz = xr;
                    resultado.Error = error;
                    resultado.IteracionesRealizadas = i;
                    resultado.Mensaje = "Raiz aproximada encontrada.";
                    return resultado;
                }

                xi = xd;
                fxi = fxd;
                xd = xr;
                fxd = fxr;
            }

            resultado.IteracionesRealizadas = resultado.Iteraciones.Count;

            if (resultado.Iteraciones.Count > 0)
            {
                IteracionSecante ultima =
                    resultado.Iteraciones[resultado.Iteraciones.Count - 1];

                resultado.Raiz = ultima.Xr;
                resultado.Error = ultima.Error;
            }
            else
            {
                resultado.Raiz = xd;
                resultado.Error = 0;
            }

            if (string.IsNullOrWhiteSpace(resultado.Mensaje))
            {
                resultado.Mensaje = "Se alcanzo el maximo de iteraciones.";
            }

            return resultado;
        }

        private double Evaluar(Calculo analizador, double x)
        {
            double resultado = analizador.EvaluaFx(x);

            if (double.IsNaN(resultado) || double.IsInfinity(resultado))
            {
                throw new ArgumentException("La funcion no se puede evaluar en x = " + x);
            }

            return resultado;
        }

        private double CalcularError(double actual, double anterior)
        {
            if (Math.Abs(actual) > double.Epsilon)
            {
                return Math.Abs((actual - anterior) / actual);
            }

            return Math.Abs(actual - anterior);
        }
    }
}
