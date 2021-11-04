﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Xamarin.Forms;

namespace Kit.Forms.Services
{
    public static class FontCache
    {
        public static void DeleteFontCacheIfFontChanged(Type AppType)
        {
            try
            {
                var assembly = AppType.Assembly;
                var exportFontAttribute =
                    assembly.GetCustomAttributes(typeof(ExportFontAttribute), true).FirstOrDefault() as
                        ExportFontAttribute;

                if (exportFontAttribute == null) return;

                string? fontFilePath = null;
                if (Device.RuntimePlatform == Device.Android)
                {
                    fontFilePath = Path.Combine(Xamarin.Essentials.FileSystem.CacheDirectory,
                        exportFontAttribute.FontFileName);
                }
                else if (Device.RuntimePlatform == Device.UWP)
                {
                    fontFilePath = Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "fonts",
                        exportFontAttribute.FontFileName);
                }
                else if (Device.RuntimePlatform == Device.iOS)
                {
                    fontFilePath = Path.Combine(Xamarin.Essentials.FileSystem.CacheDirectory,
                        exportFontAttribute.FontFileName);
                }

                if (string.IsNullOrEmpty(fontFilePath)) return;

                var deleteFile = false;

                var asmName = assembly.GetName().Name;
                using(ReflectionCaller caller=new ReflectionCaller(assembly))
                {
                    string fontName = exportFontAttribute.FontFileName;
                    string resourceName = caller.FindResources(x=>x.Contains(fontName)).FirstOrDefault();

                    using (Stream embeddedStream =caller.GetResource(resourceName))
                    {
                        using (var fileStream =
                             File.Exists(fontFilePath) ?
                            File.OpenRead(fontFilePath) : null)
                        {
                            var embeddedFontHash = GetHash(embeddedStream);
                            var cachedFontHash = GetHash(fileStream);

                            deleteFile = embeddedFontHash is null || !embeddedFontHash.SequenceEqual(cachedFontHash);
                        }

                        if (deleteFile)
                        {
                            Debug.WriteLine($"deleting '{fontFilePath}'");
                            File.Delete(fontFilePath);
                            using (var fileStream = File.Open(fontFilePath, FileMode.OpenOrCreate))
                            {
                                embeddedStream.Position = 0;
                                fileStream.Position = 0;
                                embeddedStream.CopyTo(fileStream);
                            }
                        }
                    }
                }

                static byte[] GetHash(Stream stream)
                {
                    if (stream is null)
                    {
                        return null;
                    }
                    using (SHA256 hashAlgorithm = SHA256.Create())
                    {
                        byte[] data = hashAlgorithm.ComputeHash(stream);
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "FontCache");
            }
        }
    }
}