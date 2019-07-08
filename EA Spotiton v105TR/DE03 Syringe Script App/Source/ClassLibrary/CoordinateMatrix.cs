using System;


namespace EA.PixyControl.ClassLibrary
{
    public class CoordinateMatrix : MachineCoordinate
    {
        private int xsteps;
        private int ysteps;
        private double xstepsize;
        private double ystepsize;

        #region properties

        public int XSteps
        {
            get { return xsteps; }
            set { xsteps = value; }
        }

        public int YSteps
        {
            get { return ysteps; }
            set { ysteps = value; }
        }

        public double XStepSize
        {
            get { return xstepsize; }
            set { xstepsize = value; }
        }

        public double YStepSize
        {
            get { return ystepsize; }
            set { ystepsize = value; }
        }

        // an indexer
        public MachineCoordinate this[int XIndex, int YIndex]
        {
            get
            {
                if (!IndexIsValid(XIndex, YIndex)) throw new Exception("Invalid X, Y index");
                return new MachineCoordinate(base.Name, base.X + XIndex * XStepSize, base.Y + YIndex * YStepSize, base.Z);
            }
        }

        #endregion

        public bool IndexIsValid(int XIndex, int YIndex)
        {
            return (XIndex >= 0 && XIndex < XSteps) && (YIndex >= 0 && YIndex < YSteps);
        }

        #region constructors

        public CoordinateMatrix(string PointName, double Xcoord, double Ycoord, double Zcoord, int XStepCount, double XIncrement, int YStepCount, double YIncrement)
            : base(PointName, Xcoord, Ycoord, Zcoord)
        {
            xsteps = XStepCount;
            ysteps = YStepCount;
            xstepsize = XIncrement;
            ystepsize = YIncrement;
        }


        public CoordinateMatrix(MachineCoordinate Origin, int XStepCount, double XIncrement, int YStepCount, double YIncrement) : this(Origin.Name, Origin.X, Origin.Y, Origin.Z, XStepCount, XIncrement, YStepCount, YIncrement) { }

        public CoordinateMatrix(CoordinateMatrix rhs) : this(rhs.Name, rhs.X, rhs.Y, rhs.Z, rhs.XSteps, rhs.XStepSize, rhs.YSteps, rhs.YStepSize) { }

        public CoordinateMatrix() : this("", 0.0, 0.0, 0.0, 0, 0.0, 0, 0.0) { }

        #endregion

        public override string ToString()
        {
            return string.Format("({0:F3}, {1:F3}, {2:F3}) XSteps: {3}, XIncr: {4:F3}, YSteps: {5}, YIncr: {6:F3}", X, Y, Z, xsteps, xstepsize, ysteps, ystepsize);
        }
    }
}