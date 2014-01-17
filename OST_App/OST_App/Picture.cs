using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace OST_App
{
    class Picture
    {
        private int id;
        public String path;
        private String _originalPath;

        /// <summary>
        ///     Constructor for specifying the Picture with its ID and path.
        /// </summary>
        /// <param name="id_">ID of the picture</param>
        /// <param name="path_">path of the picture, relative to the GAPED directory</param>
        public Picture(int id_, String path_) 
        {
            id = id_;
            _originalPath = path_;
            path = "..\\..\\..\\..\\..\\GAPED\\" + path_.Replace("/", @"\");
        }

        /// <summary>
        ///     Gets the next picture based on current picture ID. If called on last picture, first picture is returned
        /// </summary>
        /// <returns>the next picture</returns>
        public Picture GetNextPicture()
        {
            try
            {
                SQLiteDatabase db = new SQLiteDatabase();
                DataTable Picture;
                String query = "select id \"id\", path \"path\" from Picture where id > " + this.id + " limit 1;";
                Picture = db.GetDataTable(query);
                // should have only one row
                Picture currentPicture = null;
                foreach (DataRow r in Picture.Rows)
                {
                    currentPicture = new Picture(int.Parse(r["id"].ToString()), r["path"].ToString());
                }
                if (currentPicture == null)
                {
                    // if on last picture
                    return GetFirstPicture();
                }
                return currentPicture;
            }
            catch (Exception fail)
            {
                Console.WriteLine(fail.Message.ToString());
            }
            return null;
        }

        /// <summary>
        ///     Gets the previous picture based on current picture ID. If called on first picture, last picture is returned
        /// </summary>
        /// <returns>the previous picture</returns>
        public Picture GetPreviousPicture()
        {
            try
            {
                SQLiteDatabase db = new SQLiteDatabase();
                DataTable Picture;
                String query = "select id \"id\", path \"path\" from Picture where id < " + this.id + " ORDER BY id DESC limit 1;";
                Picture = db.GetDataTable(query);
                // should have only one row
                Picture currentPicture = null;
                foreach (DataRow r in Picture.Rows)
                {
                    currentPicture = new Picture(int.Parse(r["id"].ToString()), r["path"].ToString());
                }
                if (currentPicture == null) {
                    // if on first picture
                    return GetLastPicture();
                }
                return currentPicture;
            }
            catch (Exception fail)
            {
                Console.WriteLine(fail.Message.ToString());
            }
            return null;
        }

        /// <summary>
        /// Gets first next picture that is from different category.
        /// </summary>
        /// <returns></returns>
        public Picture GetNextCategoryPicture()
        {
            try
            {
                SQLiteDatabase db = new SQLiteDatabase();
                DataTable Picture;
                String query = "select id \"id\", path \"path\" from Picture where id > " + this.id + ";";
                Picture = db.GetDataTable(query);

                String currentPicDir = Path.GetDirectoryName(this._originalPath);
                foreach (DataRow r in Picture.Rows)
                {
                    if (!Path.GetDirectoryName(r["path"].ToString()).Equals(currentPicDir))
                        return new Picture(int.Parse(r["id"].ToString()), r["path"].ToString());
                }
                // if on last picture
                return GetFirstPicture();
            }
            catch (Exception fail)
            {
                Console.WriteLine(fail.Message.ToString());
            }
            return null;
        }

        /// <summary>
        /// Get first previous picture that is from different category.
        /// </summary>
        /// <returns></returns>
        public Picture GetPrevCategoryPicture()
        {
            try
            {
                SQLiteDatabase db = new SQLiteDatabase();
                DataTable Picture;
                String query = "select id \"id\", path \"path\" from Picture where id < " + this.id + " order by id desc;";
                Picture = db.GetDataTable(query);

                String currentPicDir = Path.GetDirectoryName(this._originalPath);
                foreach (DataRow r in Picture.Rows)
                {
                    if (!Path.GetDirectoryName(r["path"].ToString()).Equals(currentPicDir))
                        return new Picture(int.Parse(r["id"].ToString()), r["path"].ToString());
                }
                // if on first picture
                return GetLastPicture();
            }
            catch (Exception fail)
            {
                Console.WriteLine(fail.Message.ToString());
            }
            return null;
        }

        /// <summary>
        ///     Gets the first picture from the DB
        /// </summary>
        /// <returns>the first picture</returns>
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

        /// <summary>
        ///     Gets the first picture from the DB
        /// </summary>
        /// <param name="name">the name for image that should be searched</param>
        /// <returns>the first picture</returns>
        static public Picture searchPicture(String name)
        {
            try
            {
                SQLiteDatabase db = new SQLiteDatabase();
                DataTable Picture;
                String query = "select id \"id\", path \"path\" from Picture;";
                Picture = db.GetDataTable(query);

                Picture currentPic = null;
                foreach (DataRow r in Picture.Rows)
                {
                    String filename = Path.GetFileName(r["path"].ToString());
                    if (filename.Substring(0, filename.LastIndexOf(".")).ToLower() == name.ToLower())
                    {
                        currentPic = new Picture(int.Parse(r["id"].ToString()), r["path"].ToString());
                        return currentPic;
                    }
                }
                return currentPic;
            }
            catch (Exception fail)
            {
                Console.WriteLine(fail.Message.ToString());
            }
            return null;
        }

        /// <summary>
        ///     Gets the last picture from the DB
        /// </summary>
        /// <returns>the last picture</returns>
        static public Picture GetLastPicture()
        {
            try
            {
                SQLiteDatabase db = new SQLiteDatabase();
                DataTable Picture;
                String query = "select id \"id\", path \"path\" from Picture order by id DESC limit 1;";
                Picture = db.GetDataTable(query);
                Picture currentPic = null;
                // should be only one row
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

        /// <summary>
        ///     Adds synset for this Picture with corresponding lex id
        /// </summary>
        /// <param name="lex_id"></param>
        /// <returns>whether the insertion succeded or not</returns>
        public bool addSynset(string lex_id)
        {
            try
            {
                SQLiteDatabase db = new SQLiteDatabase();
                DataTable Synset;
                String query = "select id from Synset where lex_id='" + lex_id + "';";
                Synset = db.GetDataTable(query);

                String synsetId;

                // if synset is in db get its id else insert and then get
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

        /// <summary>
        ///     Removes synset for this Picture with corresponding lex id
        /// </summary>
        /// <param name="lex_id"></param>
        /// <returns>whether the deletion succeded or not</returns>
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

        /// <summary>
        ///     Gets all synsets from the DB for this Picture
        /// </summary>
        /// <returns>list of all synsets for this image</returns>
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

                return synsets;
            }
            catch
            {

                return new List<string>();
            }
        }
    }
}
