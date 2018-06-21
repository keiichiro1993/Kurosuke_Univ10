using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Storage;
using Windows.UI.Popups;

namespace Kurosuke_Universal.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public class StoreCache
    {
        /// <summary>
        /// キャッシュを保存
        /// </summary>
        /// <param name="key">キー（正確にはファイル名）</param>
        /// <param name="storeObject">保存対象のオブジェクト</param>
        /// <returns>成功すればtrue</returns>
        public async Task<bool> SaveCache(string key, object storeObject)
        {
            try
            {
                var json = JsonConvert.SerializeObject(storeObject);
                StorageFolder folder = ApplicationData.Current.LocalCacheFolder;
                StorageFile file = await folder.CreateFileAsync(key, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, json);
                return true;
            }
            catch (Exception ex)
            {
                var message = new MessageDialog("キャッシュの操作中にエラーが発生しました。：\n" + ex.Message, "おや？なにかがおかしいようです。");
                await message.ShowAsync();
                return false;
            }
        }

        /// <summary>
        /// キャッシュを読み込み
        /// </summary>
        /// <typeparam name="T">ロードするキャッシュのクラス</typeparam>
        /// <param name="key">キー（正確にはファイル名）</param>
        /// <returns>見つからなければnullを返す。</returns>
        public async Task<T> TryLoadCache<T>(string key)
            where T : class
        {
            StorageFolder folder = ApplicationData.Current.LocalCacheFolder;
            if ((await folder.GetFilesAsync()).Where(q => q.Name == key).Any())
            {
                try
                {
                    StorageFile file = await folder.GetFileAsync(key);
                    var text = await FileIO.ReadTextAsync(file);
                    return JsonConvert.DeserializeObject<T>(text);
                }
                catch (Exception ex)
                {
                    var message = new MessageDialog("キャッシュの操作中にエラーが発生しました。：\n" + ex.Message, "おや？なにかがおかしいようです。");
                    await message.ShowAsync();
                    return null;
                }
            }
            else
            {
                return null;
            }

        }
    }
}
