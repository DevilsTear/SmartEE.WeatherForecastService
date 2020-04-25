using System;
using System.Collections;
using System.Data;
using System.Data.SQLite;

/// <summary>
/// DBlite yardımcı sınıfı
/// </summary>
public class DBlite
{
    /// <summary>
    /// Veritabanı adı
    /// </summary>
    public static string DBName = "service.db";
    /// <summary>
    /// Log klasör yolu
    /// </summary>
    public static string LogDirectory = "logs";
    /// <summary>
    /// Bağlantı dizisi
    /// </summary>
    public static string ConnectionString = @"Data Source=" + DBName + @";journal mode=Wal;page size=4096;cache size=10000;synchronous=Normal;";
    /// <summary>
    /// SQLite bağlantı nesnesi
    /// </summary>
    public static SQLiteConnection con = new SQLiteConnection(ConnectionString);

    /// <summary>
    /// SQL scriptini çalıştırır
    /// </summary>
    /// <param name="sql">Çalıştırılacak SQL scripti</param>
    /// <returns>İşlem görmüş olan kayıt sayısını geri döndürür</returns>
    public static int ExecuteSQL(string sql)
    {
        int sonuc = -1;
        string sqlx = sql;
        SQLiteCommand cmd = new SQLiteCommand(sqlx, con);
        con.Open();
        try
        {
            sonuc = cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { }
        finally { con.Close(); }
        cmd.Dispose();
        return sonuc;
    }

    /// <summary>
    /// Select SQL scriptini çalıştırır
    /// </summary>
    /// <param name="sql">Çalıştırılacak SQL scripti</param>
    /// <returns>Dönen sorgu sonucunu tablo olarak geri döndürür</returns>
    public static DataTable GetData(string sql)
    {
        SQLiteDataAdapter adp = new SQLiteDataAdapter(sql, con);
        adp.SelectCommand.CommandTimeout = 120;
        DataTable dt = new DataTable();
        adp.Fill(dt);
        adp.Dispose();
        return dt;
    }

    /// <summary>
    /// Select SQL scriptini, parametreleri listesini ekleyip çalıştırır
    /// </summary>
    /// <param name="sql">Çalıştırılacak SQL scripti</param>
    /// <param name="par">Çalıştırılacak SQL parametre listesi</param>
    /// <returns>Dönen sorgu sonucunu tablo olarak geri döndürür</returns>
    public static DataTable GetData(string sql, ArrayList par)
    {
        SQLiteDataAdapter adp = new SQLiteDataAdapter(sql, con);
        foreach (SQLiteParameter p in par)
            adp.SelectCommand.Parameters.Add(p);
        DataTable dt = new DataTable();
        adp.Fill(dt);
        adp.Dispose();
        return dt;
    }

    /// <summary>
    /// SQL scriptini, parametreleri listesini ekleyip çalıştırır
    /// </summary>
    /// <param name="sql">Çalıştırılacak SQL scripti</param>
    /// <param name="par">Çalıştırılacak SQL parametre listesi</param>
    /// <returns>İşlem görmüş olan kayıt sayısını geri döndürür</returns>
    public static int ExecuteSQL(string sql, ArrayList par)
    {
        int sonuc = -1;
        SQLiteCommand cmd = new SQLiteCommand(sql, con);
        foreach (SQLiteParameter p in par)
            cmd.Parameters.Add(p);

        try
        {
            con.Open();
            sonuc = cmd.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            // throw e;
        }
        finally { con.Close(); }

        return sonuc;
    }
}