﻿using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;
using XUnity.ResourceRedirector;

namespace XUnity.AutoTranslator.Plugin.Core.AssetRedirection
{
   internal class TextAssetLoadedHandler : AssetLoadedHandlerBaseV2<TextAsset>
   {
      protected override string CalculateModificationFilePath( TextAsset asset, IAssetOrResourceLoadedContext context )
      {
         return context.GetPreferredFilePath( asset, ".txt" );
      }

      protected override bool DumpAsset( string calculatedModificationPath, TextAsset asset, IAssetOrResourceLoadedContext context )
      {
         Directory.CreateDirectory( new FileInfo( calculatedModificationPath ).Directory.FullName );
         File.WriteAllBytes( calculatedModificationPath, asset.bytes );

         return true;
      }

      protected override bool ReplaceOrUpdateAsset( string calculatedModificationPath, ref TextAsset asset, IAssetOrResourceLoadedContext context )
      {
         RedirectedResource file;

         var files = RedirectedDirectory.GetFile( calculatedModificationPath ).ToList();
         if( files.Count == 0 )
         {
            return false;
         }
         else
         {
            if( files.Count > 1 )
            {
               XuaLogger.AutoTranslator.Warn( "Found more than one resource file in the same path: " + calculatedModificationPath );
            }

            file = files.FirstOrDefault();
         }

         if( file != null )
         {
            using( var stream = file.OpenStream() )
            {
               var data = stream.ReadFully( (int)stream.Length );
               var text = Encoding.UTF8.GetString( data );

               var ext = asset.GetOrCreateExtensionData<TextAssetExtensionData>();
               ext.Data = data;
               ext.Text = text;

               return true;
            }
         }
         return false;
      }

      protected override bool ShouldHandleAsset( TextAsset asset, IAssetOrResourceLoadedContext context )
      {
         return !context.HasReferenceBeenRedirectedBefore( asset );
      }
   }
}
