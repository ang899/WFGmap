
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace WFGmap
{
    public partial class Mapform : Form
    {
        private GMapMarker currentMarker;
        string connectionString = @"Data Source = (LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Gelya\source\repos\WFGmap\Database1.mdf;Integrated Security = True";
        

        public Mapform()
        {
            InitializeComponent();
            mapLoad();
        }

        public void mapLoad()
        {
            gMapControl1.MapProvider = ArcGIS_World_Street_MapProvider.Instance; // источник карты
            GMaps.Instance.Mode = AccessMode.ServerAndCache; // режим работы GMap
            gMapControl1.Position = new PointLatLng(20, 80);
            gMapControl1.DragButton = MouseButtons.Left;// перетаскивание карты, если зажимаешь левую кнопку мыши
            markerLoad();
        }
        public void markerLoad()
        {
            string sql = "SELECT Id,X,Y from points";
            List<Point> coorglist = new List<Point>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            coorglist.Add(new Point(Convert.ToInt32(reader["Id"]),
                            Convert.ToInt32(reader["X"]), Convert.ToInt32(reader["Y"])));
                        }
                    }
                }
            }
           

            List<GMapMarker> gs = new List<GMapMarker>();
            int i = 0;
            foreach (Point item in coorglist)//добавлене маркера в список
            {
                GMapMarker marker = new GMarkerGoogle(new PointLatLng(item.X, item.Y),GMarkerGoogleType.red);
                marker.Tag = i;
                gs.Add(marker);
                i++;
            }

            GMapOverlay markers = new GMapOverlay("markers");
            foreach(GMapMarker item in gs)//добавление маркера на карту
            { 
            markers.Markers.Add(item);
            }
            gMapControl1.Overlays.Add(markers);

           }
        private void gMapControl1_MouseMove(object sender, MouseEventArgs e)
        {
            
            PointLatLng point = gMapControl1.FromLocalToLatLng(e.X, e.Y);
            label1.Text = point.Lat.ToString();
            label2.Text = point.Lng.ToString();return;
            
        }
        private void gMapControl1_MouseDown(object sender, MouseEventArgs e)
        {
            currentMarker = gMapControl1.Overlays.SelectMany(p => p.Markers).FirstOrDefault(m => m.IsMouseOver == true);
            //находим тот маркер координаты которого соответствуют положению мыши
        }
        private void gMapControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (currentMarker is null)
            {
                return;
            }
            else
            {
                var latlng = gMapControl1.FromLocalToLatLng(e.X, e.Y);//считывание координаты курсора мыши
                currentMarker.Position = latlng; //присваиваем новые коордитаты 
                currentMarker = null;//очищием значение текущего маркера   
            }
        }

        private void saveCoord()
        {
           List<Point> coorglist = new List<Point>();
            int i = 0;
           var gs = gMapControl1.Overlays.SelectMany(p => p.Markers);

            foreach (GMapMarker item in gs)
            {
                coorglist.Add(new Point(i,item.Position.Lat, item.Position.Lng));
                    i++;
            }

            //Очистка БД
            string sqldelete = "DELETE FROM points";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sqldelete, conn);
                cmd.ExecuteNonQuery();
            }
            //Вставка обновленных значений
            string sqlinsert = "INSERT INTO points VALUES(@A,@B,@C)";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                foreach (Point item in coorglist)
                {
                    using (SqlCommand cmd = new SqlCommand(sqlinsert, conn))
                    {
                        cmd.Parameters.Add("@A", SqlDbType.Int).Value = item.Id;
                        cmd.Parameters.Add("@B", SqlDbType.Decimal).Value = item.X;
                        cmd.Parameters.Add("@C", SqlDbType.Decimal).Value = item.Y;
                        cmd.ExecuteNonQuery();
                    }
                    
                }
                
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            saveCoord();
        }
    }
    
}
    