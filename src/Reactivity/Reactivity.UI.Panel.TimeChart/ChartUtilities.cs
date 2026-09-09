using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;

namespace Reactivity.UI.Panel.TimeChart
{
    /// <summary>
    /// Class that contains various utilities for drawing charts
    /// </summary>
    public static class ChartUtilities
    {
        // ********************************************************************
        // Public Methods
        // ********************************************************************
        #region Public Methods

        /// <summary>
        /// Calculates the as near to the input as possible, a power of 10 times 1,2, or 5 
        /// </summary>
        /// <param name="optimalValue"> The value to get closest to</param>
        /// <returns>The nearest value to the input value</returns>
        public static double Closest_1_2_5_Pow10(double optimalValue)
        {
            double[] numbersList = { 1.0, 2.0, 5.0 };
            return ClosestValueInListTimesBaseToInteger(optimalValue, numbersList, 10.0);
        }

        /// <summary>
        /// Calculates the closest possible value to the optimalValue passed
        /// in, that can be obtained by multiplying one of the numbers in the
        /// list by the baseValue to the power of any integer.
        /// </summary>
        /// <param name="optimalValue">The number to get closest to</param>
        /// <param name="numbers">List of numbers to mulitply by</param>
        /// <param name="baseValue">The base value</param>
        /// <returns></returns>
        public static double ClosestValueInListTimesBaseToInteger(double optimalValue, double[] numbers, double baseValue)
        {
            double multiplier = Math.Pow(baseValue, Math.Floor(Math.Log(optimalValue) / Math.Log(baseValue)));
            double minimumDifference = baseValue * baseValue * multiplier;
            double closestValue = 0.0;
            double minimumNumber = baseValue * baseValue;

            foreach (double number in numbers)
            {
                double difference = Math.Abs(optimalValue - number * multiplier);
                if (difference < minimumDifference)
                {
                    minimumDifference = difference;
                    closestValue = number * multiplier;
                }
                if (number < minimumNumber)
                {
                    minimumNumber = number;
                }
            }

            if (Math.Abs(optimalValue - minimumNumber * baseValue * multiplier) < Math.Abs(optimalValue - closestValue))
                closestValue = minimumNumber * baseValue * multiplier;

            return closestValue;
        }

        #endregion Public Methods

    }//ChartUtilities
}
