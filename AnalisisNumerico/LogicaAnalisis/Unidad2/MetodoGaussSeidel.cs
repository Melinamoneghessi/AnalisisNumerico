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

            ValidarCoeficientesIncognita(matrizAumentada, dimension);

            ResultadoGaussSeidel resultado = new ResultadoGaussSeidel
            {
                Dimension = dimension,
                VectorResultado = new double[dimension]
            };

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
                    double valor = matrizAumentada[fila, dimension];
                    double coeficienteIncognita = matrizAumentada[fila, fila];

                    for (int columna = 0; columna < dimension; columna++)
                    {
                        if (columna != fila)
                        {
                            valor = valor - (matrizAumentada[fila, columna] * resultado.VectorResultado[columna]);
                        }
                    }

                    resultado.VectorResultado[fila] = valor / coeficienteIncognita;
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
                : "Se supero el maximo de iteraciones.";

            return resultado;
        }

        private void ValidarCoeficientesIncognita(double[,] matriz, int dimension)
        {
            for (int fila = 0; fila < dimension; fila++)
            {
                if (Math.Abs(matriz[fila, fila]) < ToleranciaPivote)
                {
                    throw new ArgumentException("No se puede resolver: el coeficiente de x" + (fila + 1) + " no puede ser cero.");
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
