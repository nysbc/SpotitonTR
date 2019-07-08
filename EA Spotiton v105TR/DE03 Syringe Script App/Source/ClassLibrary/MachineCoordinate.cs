using System;


namespace EA.PixyControl.ClassLibrary
{
    public class MachineCoordinate
    {
        #region members

        private string name;
        private double x;
        private double y;
        private double z;

        #endregion

        #region properties

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public double X
        {
            get { return x; }
            set { x = value; }
        }

        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        public double Z
        {
            get { return z; }
            set { z = value; }
        }

        public double this[int Ind]
        {
            get
            {
                switch (Ind)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    default:
                        return 0.0;
                }
            }
            set
            {
                switch (Ind)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    case 2:
                        Z = value;
                        break;
                }
            }
        }

        #endregion

        #region constructors

        public MachineCoordinate(string PointName, double Xcoord, double Ycoord, double Zcoord)
        {
            name = PointName;
            x = Xcoord;
            y = Ycoord;
            z = Zcoord;
        }

        public MachineCoordinate(MachineCoordinate rhs) : this(rhs.Name, rhs.X, rhs.Y, rhs.Z) { }

        public MachineCoordinate(double Xcoord, double Ycoord, double Zcoord) : this("", Xcoord, Ycoord, Zcoord) { }

        public MachineCoordinate() : this("", 0.0, 0.0, 0.0) { }

        #endregion

        #region operators

        public static MachineCoordinate operator +(MachineCoordinate lhs, MachineCoordinate rhs)
        {
            return new MachineCoordinate(rhs.X + lhs.X, rhs.Y + lhs.Y, rhs.Z + lhs.Z);  //pk24may (z was wrong)
        }

        public static MachineCoordinate operator -(MachineCoordinate lhs, MachineCoordinate rhs)
        {
            return new MachineCoordinate(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);  //pk24may (again ??)
        }

        public static bool operator ==(MachineCoordinate lhs, MachineCoordinate rhs)
        {
            if (ReferenceEquals(lhs, null)) return false;
            if (ReferenceEquals(rhs, null)) return false; 
            return ((rhs.X == lhs.X) && (rhs.Y == lhs.Y) && (rhs.Z == lhs.Z));
        }

        public static bool operator !=(MachineCoordinate lhs, MachineCoordinate rhs)
        {
            return !(lhs == rhs);
        }

        #endregion

        #region object overrides

        public override string ToString()
        {
            return string.Format("({0:F3}, {1:F3}, {2:F3})", x, y, z);
        }

        public override bool Equals(object obj)
        {
            // if they're the same place in memory, then they're equal
            if (ReferenceEquals(this, obj)) return true;

            // make sure it's the right type - otherwise they're not equal
            MachineCoordinate rhs = obj as MachineCoordinate;
            if (ReferenceEquals(rhs, null)) return false;

            // use the == operator to test for equal values
            return (this == rhs);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        #endregion

    }

}