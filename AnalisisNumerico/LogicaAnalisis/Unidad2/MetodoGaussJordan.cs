using System;
using System.Globalization;
using System.Text;

namespace LogicaAnalisis.Unidad2
{
    public class MetodoGaussJordan
    {
        private const double ToleranciaPivote = 0.000000000001;

        public ResultadoGaussJordan Calcular(double[,] matrizAumentada)
        {
            if (matrizAumentada == null)
            {
                throw new ArgumentException("Debe ingresar una matriz aumentada.");
            }

            int filas = matrizAumentada.GetLength(0);
            int columnas = matrizAumentada.GetLength(1);

            if (filas <= 0 || columnas != filas + 1)
            {
                throw new ArgumentException("La matriz debe tener dimension n x (n + 1).");
            }

            double[,] matriz = CopiarMatriz(matrizAumentada);
            double[,] coeficientes = ExtraerCoeficientes(matrizAumentada);

            ResultadoGaussJordan resultado = new ResultadoGaussJordan
            {
                Dimension = filas
            };

            int paso = 1;

            for (int filaPivote = 0; filaPivote < filas; filaPivote++)
            {
                if (Math.Abs(matriz[filaPivote, filaPivote]) < ToleranciaPivote)
                {
                    IntercambiarConFilaValida(matriz, filaPivote);
                }

                double pivote = matriz[filaPivote, filaPivote];

                if (Math.Abs(pivote) < ToleranciaPivote)
                {
                    throw new ArgumentException("No se puede resolver: el sistema no tiene pivote valido en la fila " + (filaPivote + 1) + ".");
                }

                for (int columna = 0; columna < columnas; columna++)
                {
                    matriz[filaPivote, columna] = matriz[filaPivote, columna] / pivote;
                }

                AgregarIteracion(
                    resultado,
                    paso++,
                    filaPivote,
                    filaPivote,
                    pivote,
                    "Dividir F" + (filaPivote + 1) + " por " + FormatearNumero(pivote),
                    matriz
                );

                for (int filaActual = 0; filaActual < filas; filaActual++)
                {
                    if (filaActual == filaPivote)
                    {
                        continue;
                    }

                    double coeficienteCero = matriz[filaActual, filaPivote];

                    if (Math.Abs(coeficienteCero) < ToleranciaPivote)
                    {
                        matriz[filaActual, filaPivote] = 0;
                        continue;
                    }

                    for (int columna = 0; columna < columnas; columna++)
                    {
                        matriz[filaActual, columna] =
                            matriz[filaActual, columna] -
                            (coeficienteCero * matriz[filaPivote, columna]);
                    }

                    matriz[filaActual, filaPivote] = 0;

                    AgregarIteracion(
                        resultado,
                        paso++,
                        filaPivote,
                        filaPivote,
                        1,
                        "F" + (filaActual + 1) + " = F" + (filaActual + 1) + " - (" + FormatearNumero(coeficienteCero) + " * F" + (filaPivote + 1) + ")",
                        matriz
                    );
                }
            }

            resultado.MatrizFinal = matriz;
            resultado.VectorResultado = ObtenerVectorResultado(matriz);
            resultado.NumeroCondicion = CalcularNumeroCondicionInfinito(coeficientes);
            resultado.Condicionamiento = ClasificarCondicionamiento(resultado.NumeroCondicion);
            resultado.Mensaje = "Sistema resuelto correctamente.";

            return resultado;
        }

        private void IntercambiarConFilaValida(double[,] matriz, int filaPivote)
        {
            int filas = matriz.GetLength(0);
            int columnas = matriz.GetLength(1);

            for (int fila = filaPivote + 1; fila < filas; fila++)
            {
                if (Math.Abs(matriz[fila, filaPivote]) >= ToleranciaPivote)
                {
                    for (int columna = 0; columna < columnas; columna++)
                    {
                        double auxiliar = matriz[filaPivote, columna];
                        matriz[filaPivote, columna] = matriz[fila, columna];
                        matriz[fila, columna] = auxiliar;
                    }

                    return;
                }
            }
        }

        private double[] ObtenerVectorResultado(double[,] matriz)
        {
            int dimension = matriz.GetLength(0);
            int ultimaColumna = matriz.GetLength(1) - 1;
            double[] vector = new double[dimension];

            for (int fila = 0; fila < dimension; fila++)
            {
                vector[fila] = matriz[fila, ultimaColumna];
            }

            return vector;
        }

        private double[,] ExtraerCoeficientes(double[,] matrizAumentada)
        {
            int dimension = matrizAumentada.GetLength(0);
            double[,] coeficientes = new double[dimension, dimension];

            for (int fila = 0; fila < dimension; fila++)
            {
                for (int columna = 0; columna < dimension; columna++)
                {
                    coeficientes[fila, columna] = matrizAumentada[fila, columna];
                }
            }

            return coeficientes;
        }

        private double CalcularNumeroCondicionInfinito(double[,] matriz)
        {
            double normaMatriz = CalcularNormaInfinito(matriz);
            double[,] inversa = CalcularInversa(matriz);
            double normaInversa = CalcularNormaInfinito(inversa);

            return normaMatriz * normaInversa;
        }

        private string ClasificarCondicionamiento(double numeroCondicion)
        {
            if (double.IsInfinity(numeroCondicion) || double.IsNaN(numeroCondicion))
            {
                return "Mal condicionado";
            }

            return numeroCondicion <= 100
                ? "Bien condicionado"
                : "Mal condicionado";
        }

        private double CalcularNormaInfinito(double[,] matriz)
        {
            int filas = matriz.GetLength(0);
            int columnas = matriz.GetLength(1);
            double norma = 0;

            for (int fila = 0; fila < filas; fila++)
            {
                double sumaFila = 0;

                for (int columna = 0; columna < columnas; columna++)
                {
                    sumaFila += Math.Abs(matriz[fila, columna]);
                }

                if (sumaFila > norma)
                {
                    norma = sumaFila;
                }
            }

            return norma;
        }

        private double[,] CalcularInversa(double[,] matriz)
        {
            int dimension = matriz.GetLength(0);
            double[,] aumentada = new double[dimension, dimension * 2];
            double[,] inversa = new double[dimension, dimension];

            for (int fila = 0; fila < dimension; fila++)
            {
                for (int columna = 0; columna < dimension; columna++)
                {
                    aumentada[fila, columna] = matriz[fila, columna];
                }

                aumentada[fila, dimension + fila] = 1;
            }

            for (int filaPivote = 0; filaPivote < dimension; filaPivote++)
            {
                int filaMayor = BuscarFilaMayorPivote(aumentada, filaPivote, dimension);

                if (Math.Abs(aumentada[filaMayor, filaPivote]) < ToleranciaPivote)
                {
                    throw new ArgumentException("No se puede calcular el condicionamiento: la matriz de coeficientes es singular.");
                }

                if (filaMayor != filaPivote)
                {
                    IntercambiarFilas(aumentada, filaPivote, filaMayor);
                }

                double pivote = aumentada[filaPivote, filaPivote];

                for (int columna = 0; columna < dimension * 2; columna++)
                {
                    aumentada[filaPivote, columna] /= pivote;
                }

                for (int filaActual = 0; filaActual < dimension; filaActual++)
                {
                    if (filaActual == filaPivote)
                    {
                        continue;
                    }

                    double coeficienteCero = aumentada[filaActual, filaPivote];

                    for (int columna = 0; columna < dimension * 2; columna++)
                    {
                        aumentada[filaActual, columna] -= coeficienteCero * aumentada[filaPivote, columna];
                    }
                }
            }

            for (int fila = 0; fila < dimension; fila++)
            {
                for (int columna = 0; columna < dimension; columna++)
                {
                    inversa[fila, columna] = aumentada[fila, dimension + columna];
                }
            }

            return inversa;
        }

        private int BuscarFilaMayorPivote(double[,] matriz, int filaPivote, int dimension)
        {
            int filaMayor = filaPivote;
            double mayorValor = Math.Abs(matriz[filaPivote, filaPivote]);

            for (int fila = filaPivote + 1; fila < dimension; fila++)
            {
                double valor = Math.Abs(matriz[fila, filaPivote]);

                if (valor > mayorValor)
                {
                    filaMayor = fila;
                    mayorValor = valor;
                }
            }

            return filaMayor;
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

        private void AgregarIteracion(
            ResultadoGaussJordan resultado,
            int paso,
            int filaPivote,
            int columnaPivote,
            double pivote,
            string operacion,
            double[,] matriz)
        {
            resultado.Iteraciones.Add(new IteracionGaussJordan
            {
                Paso = paso,
                FilaPivote = filaPivote + 1,
                ColumnaPivote = columnaPivote + 1,
                Pivote = pivote,
                Operacion = operacion,
                Matriz = ConvertirMatrizATexto(matriz)
            });
        }

        private string ConvertirMatrizATexto(double[,] matriz)
        {
            StringBuilder texto = new StringBuilder();
            int filas = matriz.GetLength(0);
            int columnas = matriz.GetLength(1);

            for (int fila = 0; fila < filas; fila++)
            {
                texto.Append("[ ");

                for (int columna = 0; columna < columnas; columna++)
                {
                    texto.Append(FormatearNumero(matriz[fila, columna]).PadLeft(12));

                    if (columna == columnas - 2)
                    {
                        texto.Append(" |");
                    }
                }

                texto.Append(" ]");

                if (fila < filas - 1)
                {
                    texto.AppendLine();
                }
            }

            return texto.ToString();
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
