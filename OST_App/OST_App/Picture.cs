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

        static public Picture GetLastPicture()
        {
            try
            {
                SQLiteDatabase db = new SQLiteDatabase();
                DataTable Picture;
                String query = "select id \"id\", path \"path\" from Picture order by id DESC limit 1;";
                Picture = db.GetDataTable(query);
                Picture currentPic = null;
                foreach (DataRow r in Picture.Rows)
                {
                    currentPic = new Picture(int.Parse(r["id"].ToString()), r["path"].ToString());
                }
                return currentPic;
            }
            catch (Exception fail)
            {
                Console.WriteLine(fail.Message.ToString());
            }
            return null;
        }

        // first get sysnset id
        // return lex_id
        public bool addSynset(string lex_id)
        {
            try
            {
                SQLiteDatabase db = new SQLiteDatabase();
                DataTable Synset;
                String query = "select id from Synset where lex_id='" + lex_id + "';";
                Synset = db.GetDataTable(query);

                String synsetId;

                // if synset is in db get his id else insert and then get
                try
                {
                    synsetId = (Synset.Rows[0])["id"].ToString();
                }
                catch
                {
                    Dictionary<String, String> insdata = new Dictionary<String, String>();
                    insdata.Add("lex_id", lex_id);
                    db.Insert("Synset", insdata);

                    String querytwo = "select id from Synset where lex_id='" + lex_id + "';";
                    Synset = db.GetDataTable(querytwo);
                    synsetId = (Synset.Rows[0])["id"].ToString();
                }

                Dictionary<String, String> data = new Dictionary<String, String>();
                data.Add("pic_id", id.ToString());
                data.Add("syn_id", synsetId);
                db.Insert("Pic_Syn", data);

                return true;
            }
            catch (Exception fail)
            {
                Console.WriteLine(fail.Message.ToString());
                return false;
            }

        }

        public bool removeSynset(string lex_id)
        {
            try
            {
                SQLiteDatabase db = new SQLiteDatabase();
                DataTable Synset;
                String query = "select id from Synset where lex_id='" + lex_id + "';";
                Synset = db.GetDataTable(query);

                String synsetId = (Synset.Rows[0])["id"].ToString();
                db.Delete("Pic_Syn", String.Format("pic_id = {0} and syn_id = {1}", id, synsetId));

                return true;
            }
            catch (Exception fail)
            {
                Console.WriteLine(fail.Message.ToString());
                return false;
            }
        }

        public List<String> getSynsets()
        {
            try
            {
                SQLiteDatabase db = new SQLiteDatabase();
                DataTable Synset;
                String query = "select syn_id from Pic_Syn where pic_id='" + id.ToString() + "';";
                Synset = db.GetDataTable(query);

                List<String> synsets = new List<String>();
                foreach (DataRow r in Synset.Rows)
                {
                    DataTable Lex;
                    String queryTwo = "select lex_id from Synset where id =" + r["syn_id"].ToString() + ";";
                    Lex = db.GetDataTable(queryTwo);

                    synsets.Add(Lex.Rows[0]["lex_id"].ToString());
                }

                Console.WriteLine(synsets);
                return synsets;
            }
            catch
            {

                return new List<string>();
            }
        }
    }
}
