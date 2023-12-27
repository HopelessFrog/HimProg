using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.VisualElements;
using PropertyChanged;
using SkiaSharp;

namespace HimProg
{
    public  class MainViewModel : ViewModelBase
    {
        private double TimeNow;

        public MainViewModel(EventDelegate eventDelegate)
        {
            this.eventDelegate = eventDelegate;

            

            ReactionParam = new ReactionParam();


            ObservableA = new ObservableValue { Value = 0 };
            ObservableB = new ObservableValue { Value = 0 };
            ObservableC = new ObservableValue { Value = 0 };

            SeriesA = GaugeGenerator.BuildSolidGauge(
                new GaugeItem(ObservableA, series =>
                {
                    series.Fill = new SolidColorPaint(s_blue, 2);
                    series.Name = "A";
                    series.DataLabelsPosition = PolarLabelsPosition.Start;
                    series.DataLabelsFormatter =
                        point => $"{point.Coordinate.PrimaryValue} моль/л ";
                }));
            SeriesB = GaugeGenerator.BuildSolidGauge(
                new GaugeItem(ObservableB, series =>
                {
                    series.Fill = new SolidColorPaint(s_red, 2);
                    series.Name = "B";
                    series.DataLabelsPosition = PolarLabelsPosition.Start;
                    series.DataLabelsFormatter =
                        point => $"{point.Coordinate.PrimaryValue} моль/л ";
                }));
            SeriesC = GaugeGenerator.BuildSolidGauge(
                new GaugeItem(ObservableC, series =>
                {
                    series.Fill = new SolidColorPaint(s_green, 2);
                    series.Name = "C";
                    series.DataLabelsPosition = PolarLabelsPosition.Start;
                    series.DataLabelsFormatter =
                        point => $"{point.Coordinate.PrimaryValue} моль/л ";
                }));

            observableValuesA = new ObservableCollection<ObservablePoint>();
            observableValuesB = new ObservableCollection<ObservablePoint>();
            observableValuesC = new ObservableCollection<ObservablePoint>();

            SeriesChart = new ObservableCollection<ISeries>
            {
                new LineSeries<ObservablePoint>
                {
                    Values = observableValuesA,
                    Stroke = new SolidColorPaint(s_blue,2),
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness = 4,
                    GeometryStroke = new SolidColorPaint(s_blue,2)

                },
                new LineSeries<ObservablePoint>
                {
                    Values = observableValuesB,
                    Stroke = new SolidColorPaint(s_red,2),
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness = 4
                },
                new LineSeries<ObservablePoint>
                {
                    Values = observableValuesC,
                    Stroke = new SolidColorPaint(s_green,2),
                    GeometrySize = 0,
                    LineSmoothness = 4,
                    Fill = null
                }

            };
        }

        
        public ICommand RuntimeRecalculate
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    if (NoBusy)
                        return;
                    Stop.Execute(this);
                    ReactionParam.Ca = 0;
                    ReactionParam.Cb = 0;
                    ReactionParam.Cc = 0;
                    (var time, var y1, var y2, var y3) = GetDataCalc.GetData(ReactionParam);
                    if (time.Length < 2 || y1.Length < 2 || y2.Length < 2 || y3.Length < 2)
                        return;

                    T = time.SkipWhile(t => t <= observableValuesA.Last().X).ToList();
                    A = y1.ToList().Skip(2000 - T.Count).ToList();
                    B = y2.ToList().Skip(2000 - T.Count).ToList();
                    C = y3.ToList().Skip(2000 - T.Count).ToList();
                    if (T.Count < 2 || A.Count < 2 || B.Count < 2 || C.Count < 2)
                        return;
                    MaxA = A.Max() + 0.00001;
                    MaxB = B.Max() + 0.00001;
                    MaxC = C.Max() + 0.00001;


                    cancelToken = new CancellationTokenSource();
                    cancel = cancelToken.Token;
                    Busy = true;
                    var qwe = await Task.Factory.StartNew(DrawChartAsync, cancelToken.Token);
                    await qwe.ContinueWith(t => Busy = false);


                });
            }
        }

        private static readonly SKColor s_blue = new(25, 118, 210);
        private static readonly SKColor s_red = new(229, 57, 53);
        private static readonly SKColor s_green = new(10, 167, 0);

        private EventDelegate eventDelegate;
        public ReactionParam ReactionParam { get; set; }
        public bool Pause { get; set; }

        public double MaxA { get; set; }
        public double MaxB { get; set; }
        public double MaxC { get; set; }

        public bool IsChartAsync { get; set; } = true;
        public int Speed { get; set; }
        public bool NoBusy 
        {
            get
            {
                return !Busy;
            }
           
        } 
        public bool Busy { get; set; } = false;
        public ObservableValue ObservableA { get; set; }
        public ObservableValue ObservableB { get; set; }
        public ObservableValue ObservableC { get; set; }

        public IEnumerable<ISeries> SeriesA { get; set; }
        public IEnumerable<ISeries> SeriesB { get; set; }
        public IEnumerable<ISeries> SeriesC { get; set; }

        private ObservableCollection<ObservablePoint> observableValuesA;
        private ObservableCollection<ObservablePoint> observableValuesB;
        private ObservableCollection<ObservablePoint> observableValuesC;

        public ObservableCollection<ISeries> SeriesChart { get; set; }

       
        private List<double> A;
        private List<double> B;
        private List<double> C;
        private List<double> T;

        private CancellationTokenSource cancelToken;
        private CancellationToken cancel;


        public ICommand Play
        {
            get
            {
                return new DelegateCommand(TryDrawChart);
            }
        }

        private void ContinueWith()
        {

        }

        private async void TryDrawChart()
        {
            if (Busy) 
                return;
            ReactionParam.Ca = 0;
            ReactionParam.Cb = 0;
            ReactionParam.Cc = 0;

            (var time, var y1, var y2, var y3) = GetDataCalc.GetData(ReactionParam);

            UpdateChart();
            T = time.ToList();
            A = y1.ToList();
            B = y2.ToList();
            C = y3.ToList();

            MaxA = A.Max()+0.0000001;
            MaxB = B.Max() + 0.0000001;
            MaxC = C.Max() + 0.0000001;

            if (IsChartAsync)
            {
                cancelToken = new CancellationTokenSource();
                cancel = cancelToken.Token;
                Busy = true;
                var qwe = await Task.Factory.StartNew(DrawChartAsync, cancelToken.Token);
                await qwe.ContinueWith(t => Busy = false);
            }
            else
            {
                DrawChart();
            }



        }

        public ICommand Stop
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if(!Busy)
                        return;
                    cancelToken.Cancel();

                });
            }
        }
        public ICommand Clear
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    if (Busy)
                        cancelToken.Cancel();
                    UpdateChart();
                   

                });
            }
        }

        private void DrawChart()
        {
            for (int i = 0; i < T.Count; i++)
            {
                addA(A[i], T[i]);
                addB(B[i], T[i]);
                addC(C[i], T[i]);
            }
        }
        private async  Task<int> DrawChartAsync()
        {
            for (int i = 0; i < T.Count; i++)
            {

                if (i != 0)
                {
                    var waitTime = 1000 * T[i] * 60;
                    if (i != 0)
                        waitTime -= T[i - 1] * 60 * 1000;
                    if (Speed < 0)
                        waitTime = waitTime * Math.Abs(Speed);
                    if (Speed > 0)
                        waitTime = waitTime * 1 / Speed;


                    await Task.Delay((int)waitTime, cancel);
                    while (Pause)
                    {
                        await Task.Delay(50);
                        if (cancel.IsCancellationRequested)
                        {
                            Pause = false;
                            return 0;
                        }
                    }
                }

                addA(A[i], T[i]);
                addB(B[i], T[i]);
                addC(C[i], T[i]);
               
                

            }

            return 0;
        }

        private void UpdateChart()
        {
            
            ObservableA.Value = 0;
            ObservableB.Value = 0;
            ObservableC.Value = 0;

            observableValuesA.Clear();
            observableValuesB.Clear();
            observableValuesC.Clear();

           
        }
        public MainViewModel()
        {

           
        }

        public Axis[] XAxes { get; set; } =
        {
            new Axis
            {
                Name = "Время (мин)",
                LabelsPaint = new SolidColorPaint(SKColors.Black),
                Labeler = value => value.ToString("N2")
            }
        };

        public Axis[] YAxes { get; set; } =
        {
            new Axis
            {
                Name = "Концетрация (моль/л)",
               
            }
        };

        private void addA(double value, double time)
        {
            observableValuesA.Add(new(time, value));
            ObservableA.Value = value;
        }
        private void addB(double value, double time)
        {
            observableValuesB.Add(new(time,value));
           ObservableB.Value = value;
        }
        private void addC(double value, double time)
        {
            observableValuesC.Add(new( time,value));
            ObservableC.Value = value;
        }


    }
}
