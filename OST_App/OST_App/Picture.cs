using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;

namespace OST_App
{
    class Picture
    {
        private int id;
        public String path;

        public Picture(int id_, String path_) 
        {
            id = id_;
            path = "..\\..\\..\\..\\GAPED\\" + path_.Replace("/", @"\");
            Console.WriteLine(path);
        }

        static public Picture GetFirstPicture() 
        {
            try {
                SQLiteDatabase db = new SQLiteDatabase();
                DataTable Picture;
                String query = "select id \"id\", path \"path\" from Picture limit 1;";
                Picture = db.GetDataTable(query);
                // should have only one row
                foreach (DataRow r in Picture.Rows)
                {
                    return new Picture(int.Parse(r["id"].ToString()), r["path"].ToString());
                }
            } catch (Exception fail) {
                Console.WriteLine(fail.Message.ToString());
            }
            return null;
        }
    }
}
