using System;
using System.Globalization;
using System.Text;

namespace LogicaAnalisis.Unidad2
{
    public class MetodoGaussSeidel
    {
        private const double Tolerancia = 0.0001;
        private const double ToleranciaPivote = 0.000000000001;
        private const int MaximoIteraciones = 100;

        public ResultadoGaussSeidel Calcular(double[,] matrizAumentada)
        {
            if (matrizAumentada == null)
            {
                throw new ArgumentException("Debe ingresar una matriz aumentada.");
            }

            int dimension = matrizAumentada.GetLength(0);
            int columnas = matrizAumentada.GetLength(1);

            if (dimension <= 0 || columnas != dimension + 1)
            {
                throw new ArgumentException("La matriz debe tener dimension n x (n + 1).");
            }

            ValidarFilasSinInformacion(matrizAumentada, dimension);

            ResultadoGaussSeidel resultado = new ResultadoGaussSeidel
            {
                Dimension = dimension,
                VectorResultado = new double[dimension]
            };

            double[,] matrizTrabajo = CopiarMatriz(matrizAumentada);
            ReordenarFilasParaGaussSeidel(matrizTrabajo, resultado);
            ValidarCoeficientesIncognita(matrizTrabajo, dimension);

            double[] vectorAnterior = new double[dimension];
            bool solucion = false;
            int contador = 0;

            while (contador < MaximoIteraciones && !solucion)
            {
                contador++;

                if (contador > 1)
                {
                    resultado.VectorResultado.CopyTo(vectorAnterior, 0);
                }

                for (int fila = 0; fila < dimension; fila++)
                {
                    double valor = matrizTrabajo[fila, dimension];
                    double coeficienteIncognita = matrizTrabajo[fila, fila];

                    for (int columna = 0; columna < dimension; columna++)
                    {
                        if (columna != fila)
                        {
                            valor = valor - (matrizTrabajo[fila, columna] * resultado.VectorResultado[columna]);
                        }
                    }

                    resultado.VectorResultado[fila] = valor / coeficienteIncognita;

                    if (double.IsInfinity(resultado.VectorResultado[fila]) || double.IsNaN(resultado.VectorResultado[fila]))
                    {
                        throw new ArgumentException("No se puede resolver por Gauss-Seidel: se obtuvo un valor no numerico al despejar x" + (fila + 1) + ".");
                    }
                }

                double mayorErrorRelativo = 0;
                int contadorMismoResultado = 0;

                for (int i = 0; i < dimension; i++)
                {
                    double errorRelativo = CalcularErrorRelativo(resultado.VectorResultado[i], vectorAnterior[i]);
                    mayorErrorRelativo = Math.Max(mayorErrorRelativo, errorRelativo);

                    if (errorRelativo <= Tolerancia)
                    {
                        contadorMismoResultado++;
                    }
                }

                solucion = contadorMismoResultado == dimension;

                resultado.Iteraciones.Add(new IteracionGaussSeidel
                {
                    Paso = contador,
                    VectorResultado = ConvertirVectorATexto(resultado.VectorResultado),
                    ErrorRelativo = mayorErrorRelativo,
                    Converge = solucion
                });
            }

            resultado.Converge = solucion;
            resultado.Mensaje = solucion
                ? "Sistema resuelto correctamente."
                : "No se pudo resolver con Gauss-Seidel: se supero el maximo de " + MaximoIteraciones + " iteraciones. El metodo no converge para este sistema o necesita otra reordenacion.";

            return resultado;
        }

        private void ValidarFilasSinInformacion(double[,] matriz, int dimension)
        {
            for (int fila = 0; fila < dimension; fila++)
            {
                bool todosCoeficientesCero = true;

                for (int columna = 0; columna < dimension; columna++)
                {
                    if (Math.Abs(matriz[fila, columna]) >= ToleranciaPivote)
                    {
                        todosCoeficientesCero = false;
                        break;
                    }
                }

                if (!todosCoeficientesCero)
                {
                    continue;
                }

                if (Math.Abs(matriz[fila, dimension]) >= ToleranciaPivote)
                {
                    throw new ArgumentException("No se puede resolver: el sistema es incompatible porque la fila " + (fila + 1) + " queda 0 = " + FormatearNumero(matriz[fila, dimension]) + ".");
                }

                throw new ArgumentException("No se puede resolver: el sistema tiene infinitas soluciones porque la fila " + (fila + 1) + " queda 0 = 0.");
            }
        }

        private void ReordenarFilasParaGaussSeidel(double[,] matriz, ResultadoGaussSeidel resultado)
        {
            int dimension = matriz.GetLength(0);
            int[] orden = BuscarMejorOrdenFilas(matriz, dimension);

            if (orden == null)
            {
                throw new ArgumentException("No se puede resolver por Gauss-Seidel: no existe un intercambio de filas que deje todos los coeficientes principales distintos de cero.");
            }

            AplicarOrdenFilas(matriz, orden, resultado);
        }

        private int[] BuscarMejorOrdenFilas(double[,] matriz, int dimension)
        {
            int totalMascaras = 1 << dimension;
            int[] puntajes = new int[totalMascaras];
            int[] filaElegida = new int[totalMascaras];
            int[] mascaraAnterior = new int[totalMascaras];

            for (int i = 0; i < totalMascaras; i++)
            {
                puntajes[i] = int.MinValue;
                filaElegida[i] = -1;
                mascaraAnterior[i] = -1;
            }

            puntajes[0] = 0;

            for (int mascara = 0; mascara < totalMascaras; mascara++)
            {
                if (puntajes[mascara] == int.MinValue)
                {
                    continue;
                }

                int posicion = ContarBits(mascara);

                if (posicion >= dimension)
                {
                    continue;
                }

                for (int fila = 0; fila < dimension; fila++)
                {
                    int bitFila = 1 << fila;

                    if ((mascara & bitFila) != 0 || Math.Abs(matriz[fila, posicion]) < ToleranciaPivote)
                    {
                        continue;
                    }

                    int nuevaMascara = mascara | bitFila;
                    int nuevoPuntaje = puntajes[mascara] + CalcularPuntajeFila(matriz, fila, posicion, dimension);

                    if (nuevoPuntaje > puntajes[nuevaMascara])
                    {
                        puntajes[nuevaMascara] = nuevoPuntaje;
                        filaElegida[nuevaMascara] = fila;
                        mascaraAnterior[nuevaMascara] = mascara;
                    }
                }
            }

            int mascaraFinal = totalMascaras - 1;

            if (puntajes[mascaraFinal] == int.MinValue)
            {
                return null;
            }

            int[] orden = new int[dimension];
            int mascaraActual = mascaraFinal;

            for (int posicion = dimension - 1; posicion >= 0; posicion--)
            {
                orden[posicion] = filaElegida[mascaraActual];
                mascaraActual = mascaraAnterior[mascaraActual];
            }

            return orden;
        }

        private int CalcularPuntajeFila(double[,] matriz, int fila, int posicion, int dimension)
        {
            double diagonal = Math.Abs(matriz[fila, posicion]);
            double sumaOtros = 0;

            for (int columna = 0; columna < dimension; columna++)
            {
                if (columna != posicion)
                {
                    sumaOtros += Math.Abs(matriz[fila, columna]);
                }
            }

            int puntaje = 1;

            if (diagonal >= sumaOtros)
            {
                puntaje += 1000;
            }

            if (diagonal > sumaOtros)
            {
                puntaje += 100;
            }

            puntaje += (int)Math.Min(100, (diagonal / (sumaOtros + ToleranciaPivote)) * 10);

            if (fila == posicion)
            {
                puntaje += 1;
            }

            return puntaje;
        }

        private int ContarBits(int mascara)
        {
            int contador = 0;

            while (mascara > 0)
            {
                contador += mascara & 1;
                mascara >>= 1;
            }

            return contador;
        }

        private void AplicarOrdenFilas(double[,] matriz, int[] orden, ResultadoGaussSeidel resultado)
        {
            int dimension = matriz.GetLength(0);
            int[] ordenActual = new int[dimension];

            for (int i = 0; i < dimension; i++)
            {
                ordenActual[i] = i;
            }

            for (int posicion = 0; posicion < dimension; posicion++)
            {
                int filaBuscada = orden[posicion];
                int posicionActual = BuscarPosicion(ordenActual, filaBuscada);

                if (posicionActual == posicion)
                {
                    continue;
                }

                IntercambiarFilas(matriz, posicion, posicionActual);

                int auxiliar = ordenActual[posicion];
                ordenActual[posicion] = ordenActual[posicionActual];
                ordenActual[posicionActual] = auxiliar;

                resultado.CambiosFilas.Add("Cambio fila " + (posicion + 1) + " por fila " + (posicionActual + 1));
            }
        }

        private int BuscarPosicion(int[] vector, int valor)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                if (vector[i] == valor)
                {
                    return i;
                }
            }

            return -1;
        }

        private void IntercambiarFilas(double[,] matriz, int filaUno, int filaDos)
        {
            int columnas = matriz.GetLength(1);

            for (int columna = 0; columna < columnas; columna++)
            {
                double auxiliar = matriz[filaUno, columna];
                matriz[filaUno, columna] = matriz[filaDos, columna];
                matriz[filaDos, columna] = auxiliar;
            }
        }

        private void ValidarCoeficientesIncognita(double[,] matriz, int dimension)
        {
            for (int fila = 0; fila < dimension; fila++)
            {
                if (Math.Abs(matriz[fila, fila]) < ToleranciaPivote)
                {
                    throw new ArgumentException("No se puede resolver por Gauss-Seidel: el coeficiente principal de x" + (fila + 1) + " queda en cero despues de reordenar filas.");
                }
            }
        }

        private double CalcularErrorRelativo(double valorActual, double valorAnterior)
        {
            if (Math.Abs(valorActual) < ToleranciaPivote)
            {
                return Math.Abs(valorActual - valorAnterior);
            }

            return Math.Abs((valorActual - valorAnterior) / valorActual);
        }

        private string ConvertirVectorATexto(double[] vector)
        {
            StringBuilder texto = new StringBuilder();

            for (int i = 0; i < vector.Length; i++)
            {
                if (i > 0)
                {
                    texto.Append("; ");
                }

                texto.Append("x");
                texto.Append(i + 1);
                texto.Append(" = ");
                texto.Append(FormatearNumero(vector[i]));
            }

            return texto.ToString();
        }

        private double[,] CopiarMatriz(double[,] matriz)
        {
            int filas = matriz.GetLength(0);
            int columnas = matriz.GetLength(1);
            double[,] copia = new double[filas, columnas];

            for (int fila = 0; fila < filas; fila++)
            {
                for (int columna = 0; columna < columnas; columna++)
                {
                    copia[fila, columna] = matriz[fila, columna];
                }
            }

            return copia;
        }

        private string FormatearNumero(double numero)
        {
            if (Math.Abs(numero) < ToleranciaPivote)
            {
                numero = 0;
            }

            return numero.ToString("0.##########", CultureInfo.InvariantCulture);
        }
    }
}
