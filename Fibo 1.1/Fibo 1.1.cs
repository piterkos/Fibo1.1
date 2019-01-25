
using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;
using System.Collections.Generic;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class NewIndicator : Indicator
    {
        [Parameter("Rodzaj zagrania: ")]
        public string RodzajZagrania { get; set; }

        private DateTime czas1;
        private DateTime czas2;
        private double cena1 = 0;
        private double cena2 = 0;
        private int LiczbaKlikniec = 0;
        private int LiczbaKlikniecALT = 0;

        private DateTime czasOtwarcia;
        private double cenaOtwarcia;
        private double cenaSL;
        private double cenaTP;
        private double cenaBE;
        private double ruch1;
        private double ruch2;
        private double ruch3;
        private string wynikTrejdu;
        private double zasieg;
        private string rodzjaTrejdu;
        private double poziomBreakEven = 0.236;
        private List<string> listaObjektow;

        protected override void Initialize()
        {
            Chart.MouseUp += OnChartMouseUp;
            Chart.MouseDown += OnChartMouseDown;
            listaObjektow = new List<string>();
        }

        void OnChartMouseDown(ChartMouseEventArgs obj)
        {

        }
        void OnChartMouseUp(ChartMouseEventArgs obj)
        {
            if (LiczbaKlikniec == 0 && obj.CtrlKey)
            {
                UsunObiekt("Fibo");
                cena1 = Math.Round(obj.YValue, 4);
                czas1 = obj.TimeValue;
                LiczbaKlikniec++;
                //Chart.DrawStaticText("ceny","Ryzyko: " + RyzykoProcentowe + "\nCena 1: " + cena1, VerticalAlignment.Center, HorizontalAlignment.Left, Color.Yellow);
                //Chart.RemoveObject("Info");
            }
            else if (LiczbaKlikniec == 1 && obj.CtrlKey)
            {
                cena2 = Math.Round(obj.YValue, 4);
                czas2 = obj.TimeValue;
                NarysujFibo();
                LiczbaKlikniec++;
                UstalRodzajTrejdu(cena1, cena2);
                //Chart.DrawStaticText("ceny", "Ryzyko: " + RyzykoProcentowe + "\nCena 1: " + Math.Round(cena1,4) + "\nCena 2: " + Math.Round(cena2,4), VerticalAlignment.Center, HorizontalAlignment.Left, Color.Yellow);
            }
            else if (LiczbaKlikniec == 2 && obj.CtrlKey)
            {
                UsunObiekt("Fibo");
                //Chart.DrawStaticText("ceny", "Rozpocznij mierzenie fibo...", VerticalAlignment.Center, HorizontalAlignment.Left, Color.Yellow);
                LiczbaKlikniec = 0;
                LiczbaKlikniecALT = 0;
                Print("12,15,48,49,20");
            }
            if (LiczbaKlikniec == 2 && obj.AltKey)
            {
                if (LiczbaKlikniecALT == 0)
                {
                    czasOtwarcia = obj.TimeValue;
                    cenaOtwarcia = Math.Round(obj.YValue, 4);
                    Chart.DrawStaticText("poziomFibo", ZwrocPoziomFibo(obj) + " %", VerticalAlignment.Center, HorizontalAlignment.Center, Color.Beige);
                    LiczbaKlikniecALT++;
                }
                else if (LiczbaKlikniecALT == 1)
                {
                    ruch1 = ZwrocPoziomFibo(obj);
                    LiczbaKlikniecALT++;
                }
                else if (LiczbaKlikniecALT == 2)
                {
                    ruch2 = ZwrocPoziomFibo(obj);
                    LiczbaKlikniecALT++;
                }
                else if (LiczbaKlikniecALT == 3)
                {
                    ruch3 = ZwrocPoziomFibo(obj);
                }
            }
            if (LiczbaKlikniec == 2 && obj.ShiftKey)
            {
                UstalWynik();
                TworzLOG();
                ResetujCenyiRuchy();
            }
            TworzOpis();
        }
        private int LP = 1;
        // komentarz zbędny
        private void TworzLOG()
        {
            string target = "";
            if (wynikTrejdu == "TP")
            {
                target = (Math.Abs(ruch1) + 100).ToString();
            }
            else
            {
                target = "";
            }
            string maxSL = "";
            if (wynikTrejdu == "SL")
            {
                maxSL = ruch1.ToString();
            }
            Print(LP + ";", czasOtwarcia + ";" + rodzjaTrejdu + ";" + wynikTrejdu + "," + RodzajZagrania + ";" + target + ";" + maxSL + ";" + ruch2 + ";" + ruch3 + ";" + cena1 + ";" + cena2);
        }

        private void ResetujCenyiRuchy()
        {
            ruch1 = 0;
            ruch2 = 0;
            ruch3 = 0;
            cenaOtwarcia = 0;

        }

        private void UstalWynik()
        {
            if (ruch1 < -0.5)
            {
                wynikTrejdu = "TP";
            }
            if (ruch1 > 0.9)
            {
                wynikTrejdu = "SL";
            }
            if (wynikTrejdu == "SL" && ruch2 > poziomBreakEven && ruch2 != 0)
            {
                wynikTrejdu = "BE";
            }
            if (wynikTrejdu == "TP" && ruch2 > poziomBreakEven && ruch3 > 0.58 && ruch2 != 0 && ruch3 != 0)
            {
                wynikTrejdu = "BE";
            }
        }

        private void UstalRodzajTrejdu(double cena1, double cena2)
        {
            if (cena1 < cena2)
            {
                rodzjaTrejdu = "BUY";
            }
            else
            {
                rodzjaTrejdu = "SELL";
            }
        }
        private double ZwrocPoziomFibo(ChartMouseEventArgs _obj)
        {
            if (cena1 < cena2)
            {
                return Math.Round((cena2 - _obj.YValue) / (cena2 - cena1) * 100, 1);
            }
            else
            {
                return Math.Round((_obj.YValue - cena2) / (cena1 - cena2) * 100, 1);
            }
        }
        private void NarysujFibo()
        {
            if (cena1 != 0 && cena2 != 0)
            {
                //var fibo = Chart.DrawFibonacciRetracement("Fibo", czas1, cena1, czas2, cena2, Color.Aqua);
                if (cena1 < cena2)
                {
                    double cena61_8 = ((cena2 - cena1) * (1 - 0.59)) + cena1;
                    double cena38_2 = ((cena2 - cena1) * (1 - poziomBreakEven)) + cena1;
                    cenaBE = cena38_2;
                    double cena0_0 = cena1;
                    double cena100 = cena2;
                    double cena161_8 = ((cena2 - cena1) * (1 - 1.618)) + cena1;

                    double cena161_8FI = ((cena2 - cena1) * 1.618) + cena1;
                    cenaTP = cena161_8FI;
                    double cena261_8 = ((cena2 - cena1) * (1 - 2.618)) + cena1;
                    double cena261_8FI = ((cena2 - cena1) * 2.618) + cena1;
                    //double cenaSL = cena1 - (cena2 - cena1) * 0.05;
                    double cenaSL2 = ((cena2 - cena1) * (1 - 0.9)) + cena1;
                    cenaBE = cenaSL2;
                    listaObjektow.Add(Chart.DrawHorizontalLine("_0.0", cena0_0, Color.White).Name);
                    listaObjektow.Add(Chart.DrawHorizontalLine("100", cena100, Color.White).Name);
                    listaObjektow.Add(Chart.DrawHorizontalLine("61.8", cena61_8, Color.Green).Name);
                    listaObjektow.Add(Chart.DrawHorizontalLine("38.2", cena38_2, Color.Green, 1, LineStyle.DotsRare).Name);
                    listaObjektow.Add(Chart.DrawHorizontalLine("161.8", cena161_8, Color.Blue).Name);
                    listaObjektow.Add(Chart.DrawHorizontalLine("161.8FI", cena161_8FI, Color.Blue).Name);
                    listaObjektow.Add(Chart.DrawHorizontalLine("261.8", cena261_8, Color.BurlyWood, 1, LineStyle.DotsRare).Name);
                    listaObjektow.Add(Chart.DrawHorizontalLine("261.8FI", cena261_8FI, Color.BurlyWood, 1, LineStyle.DotsRare).Name);
                    //Chart.DrawHorizontalLine("CenaSL", cenaSL, Color.Red);
                    listaObjektow.Add(Chart.DrawHorizontalLine("CenaSL2", cenaSL2, Color.Red).Name);
                    //Chart.DrawText("Opis61.8", "61.8 %", Chart.LastVisibleBarIndex, cena61_8, Color.Aqua); // pokazuje na bieżąco
                    listaObjektow.Add(Chart.DrawText("Opis61.8", "61.8 %", czas2, cena61_8, Color.Aqua).Name);
                    listaObjektow.Add(Chart.DrawText("Opis38.2", "38.2 % - BreakEven", czas2, cena38_2, Color.White).Name);
                    listaObjektow.Add(Chart.DrawText("Opis0.0", "0 %", czas2, cena0_0, Color.Aqua).Name);
                    listaObjektow.Add(Chart.DrawText("Opis100", "100 %", czas2, cena100, Color.Aqua).Name);
                    listaObjektow.Add(Chart.DrawText("Opis161", "161.8 %", czas2, cena161_8, Color.Aqua).Name);
                    listaObjektow.Add(Chart.DrawText("Opis161FI", "161.8 %", czas2, cena161_8FI, Color.Aqua).Name);
                    listaObjektow.Add(Chart.DrawText("Opis261FI", "261.8 %", czas2, cena261_8FI, Color.Aqua).Name);
                    listaObjektow.Add(Chart.DrawText("Opis261", "261.8 %", czas2, cena261_8, Color.Aqua).Name);
                    listaObjektow.Add(Chart.DrawText("Opis261FI", "261.8 %", czas2, cena261_8FI, Color.Aqua).Name);
                    //Chart.DrawText("OpisCenaSL", "Stop Loss", czas2, cenaSL, Color.Aqua);
                    listaObjektow.Add(Chart.DrawText("OpisCenaSL2", "Stop Loss", czas2, cenaSL2, Color.Aqua).Name);
                }
                else
                {
                    double cena61_8 = ((cena1 - cena2) * (0.59)) + cena2;
                    double cena38_2 = ((cena1 - cena2) * (poziomBreakEven)) + cena2;
                    cenaBE = cena38_2;
                    double cena0_0 = cena2;
                    double cena100 = cena1;
                    double cena161_8 = ((cena1 - cena2) * (1 - 1.618)) + cena2;
                    //cenaTP = cena161_8;
                    double cena161_8FI = ((cena1 - cena2) * 1.618) + cena2;
                    cenaTP = cena161_8FI;

                    double cena261_8 = ((cena1 - cena2) * (1 - 2.618)) + cena2;
                    double cena261_8FI = ((cena1 - cena2) * 2.618) + cena2;
                    //double cenaSL = cena1 + (cena1 - cena2) * 0.1;
                    double cenaSL2 = ((cena1 - cena2) * (0.9)) + cena2;
                    cenaBE = cenaSL2;
                    
                    listaObjektow.Add(Chart.DrawHorizontalLine("0.0", cena0_0, Color.White).Name);
                    listaObjektow.Add(Chart.DrawHorizontalLine("100", cena100, Color.White).Name);
                    listaObjektow.Add(Chart.DrawHorizontalLine("61.8", cena61_8, Color.Green).Name);
                    listaObjektow.Add(Chart.DrawHorizontalLine("38.2", cena38_2, Color.Green, 1, LineStyle.DotsRare).Name);
                    listaObjektow.Add(Chart.DrawHorizontalLine("161.8", cena161_8, Color.Blue).Name);
                    listaObjektow.Add(Chart.DrawHorizontalLine("161.8FI", cena161_8FI, Color.Blue).Name);
                    listaObjektow.Add(Chart.DrawHorizontalLine("261.8", cena261_8, Color.BurlyWood, 1, LineStyle.DotsRare).Name);
                    listaObjektow.Add(Chart.DrawHorizontalLine("261.8FI", cena261_8FI, Color.BurlyWood, 1, LineStyle.DotsRare).Name);
                    // Chart.DrawHorizontalLine("CenaSL", cenaSL, Color.Red);
                    listaObjektow.Add(Chart.DrawHorizontalLine("CenaSL", cenaSL2, Color.Red).Name);
                    //Chart.DrawText("Opis61.8", "61.8 %", Chart.LastVisibleBarIndex, cena61_8, Color.Aqua); // pokazuje na bieżąco
                    listaObjektow.Add(Chart.DrawText("Opis61.8", "61.8 %", czas2, cena61_8, Color.Aqua).Name);
                    listaObjektow.Add(Chart.DrawText("Opis38.2", "38.2 % - BreakEven", czas2, cena38_2, Color.White).Name);
                    listaObjektow.Add(Chart.DrawText("Opis0.0", "0 %", czas2, cena0_0, Color.Aqua).Name);
                    listaObjektow.Add(Chart.DrawText("Opis100", "100 %", czas2, cena100, Color.Aqua).Name);
                    listaObjektow.Add(Chart.DrawText("Opis161", "161.8 %", czas2, cena161_8, Color.Aqua).Name);
                    listaObjektow.Add(Chart.DrawText("Opis161FI", "161.8 %", czas2, cena161_8FI, Color.Aqua).Name);
                    listaObjektow.Add(Chart.DrawText("Opis261", "261.8 %", czas2, cena261_8, Color.Aqua).Name);
                    listaObjektow.Add(Chart.DrawText("Opis261FI", "261.8 %", czas2, cena261_8FI, Color.Aqua).Name);
                    //Chart.DrawText("OpisCenaSL", "Stop Loss", czas2, cenaSL, Color.Aqua);
                    listaObjektow.Add(Chart.DrawText("OpisCenaSL2", "Stop Loss", czas2, cenaSL2, Color.Aqua).Name);
                }
            }
        }
        private void TworzOpis()
        {
            if (LiczbaKlikniec == 0)
            {
                Chart.DrawStaticText("Opis", "Trzymając CTRL, kliknij na wykresie, aby określić cenę początkową", VerticalAlignment.Center, HorizontalAlignment.Left, Color.AliceBlue);
                // Chart.RemoveAllObjects();
                foreach (var okjekt in listaObjektow)
                {
                    UsunObiekt(okjekt);
                }
               
              
            }
            if (LiczbaKlikniec == 1)
            {
                Chart.DrawStaticText("Opis", "Cena 1 = " + Math.Round(cena1, 4), VerticalAlignment.Center, HorizontalAlignment.Left, Color.AliceBlue);
            }
            if (LiczbaKlikniec == 2)
            {
                Chart.DrawStaticText("Opis", "Cena 1 = " + Math.Round(cena1, 4) + "\nCena 2 = " + Math.Round(cena2, 4), VerticalAlignment.Center, HorizontalAlignment.Left, Color.AliceBlue);
            }
        }
        private void UsunObiekt(string nazwaObiektu)
        {
            Chart.RemoveObject(nazwaObiektu);
        }
        public override void Calculate(int index)
        {
            // Calculate value at specified index
            // Result[index] = ...
        }

    }
}