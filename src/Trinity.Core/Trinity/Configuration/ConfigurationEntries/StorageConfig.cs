﻿// Graph Engine
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Core.Lib;
using Trinity.Storage;
using Trinity.Utilities;

namespace Trinity.Configuration
{
    public sealed class StorageConfig
    {
        #region Singleton
        static StorageConfig s_StorageConfig = new StorageConfig();
        private StorageConfig()
        {
            StorageRoot     = DefaultStorageRoot;
            TrunkCount      = c_MaxTrunkCount;
            ReadOnly        = c_DefaultReadOnly;
            StorageCapacity = c_DefaultStorageCapacityProfile;
            DefragInterval  = c_DefaultDefragInterval;
        }
        [ConfigInstance]
        public static StorageConfig Instance { get { return s_StorageConfig; } }
        [ConfigEntryName]
        internal static string ConfigEntry { get { return ConfigurationConstants.Tags.STORAGE; } }
        #endregion

        #region Private static helpers
        private static void ThrowCreatingStorageRootException(string storageroot)
        {
            throw new IOException("WARNNING: Error occurs when creating StorageRoot: " + storageroot);
        }

        private static void ThrowLargeObjectThresholdException()
        {
            throw new InvalidOperationException("LargeObjectThreshold cannot be larger than 16MB.");
        }

        private static void ThrowDisableReadOnlyException()
        {
            throw new InvalidOperationException("ReadOnly flag cannot be disabled once enabled.");
        }

        private static string DefaultStorageRoot { get { return Path.Combine(AssemblyPath.MyAssemblyPath, "storage"); } }
        #endregion

        #region Fields
        internal const int    c_MaxTrunkCount = ConfigurationConstants.DefaultValue.MAX_TRUNK_COUNT;
        internal const bool c_DefaultReadOnly = ConfigurationConstants.DefaultValue.DEFAULT_VALUE_FALSE;
        internal const ushort c_UndefinedCellType = ConfigurationConstants.DefaultValue.UNDEFINED_CELL_TYPE;
        internal const int    c_DefaultDefragInterval = ConfigurationConstants.DefaultValue.DEFAULT_DEFRAG_INTERVAL;
        internal const StorageCapacityProfile 
                              c_DefaultStorageCapacityProfile = StorageCapacityProfile.Max8G;
        internal int          m_GCParallelism = ConfigurationConstants.DefaultValue.DEFALUT_GC_PATRALLELISM;
        internal int          m_DefragInterval;
        private  string       m_StorageRoot = ConfigurationConstants.DefaultValue.BLANK;
        #endregion

        #region Properties
        /// <summary>
        /// Gets and sets value to specify the number of memory trunks in the local memory storage.
        /// </summary>
        [ConfigSetting(Optional: true)]
        public int TrunkCount
        {
            get
            {
                return CTrinityConfig.CTrunkCount();
            }

            set
            {
                CTrinityConfig.CSetTrunkCount(value);
            }
        }

        /// <summary>
        /// Gets and sets value to specify whether the local memory storatge is read-only.
        /// </summary>
        [ConfigSetting(Optional: true)]
        public bool ReadOnly
        {
            get
            {
                return CTrinityConfig.CReadOnly();
            }

            set
            {
                CTrinityConfig.CSetReadOnly(value);
            }
        }

        /// <summary>
        /// Gets and sets value to specify the local memory storage capacity profile.
        /// </summary>
        [ConfigSetting(Optional: true)]
        public StorageCapacityProfile StorageCapacity
        {
            get
            {
                return (StorageCapacityProfile)CTrinityConfig.GetStorageCapacityProfile();
            }
            set
            {
                CTrinityConfig.SetStorageCapacityProfile((int)value);
            }
        }

        /// <summary>
        /// Gets and sets the path for saving persistent storage disk images.Defaults to AssemblyPath\storage.
        /// </summary>
        [ConfigSetting(Optional: true)]
        public unsafe string StorageRoot
        {
            get
            {
                if (m_StorageRoot == null || m_StorageRoot.Length == 0)
                {
                    m_StorageRoot = DefaultStorageRoot;
                }

                if (!Directory.Exists(m_StorageRoot))
                {
                    try
                    {
                        Directory.CreateDirectory(m_StorageRoot);
                    }
                    catch (Exception)
                    {
                        ThrowCreatingStorageRootException(m_StorageRoot);
                    }
                }

                if (m_StorageRoot[m_StorageRoot.Length - 1] != Path.DirectorySeparatorChar)
                {
                    m_StorageRoot = m_StorageRoot + Path.DirectorySeparatorChar;
                }

                try
                {
                    byte[] buff = BitHelper.GetBytes(m_StorageRoot);
                    fixed (byte* p = buff)
                    {
                        CTrinityConfig.SetStorageRoot(p, buff.Length);
                    }
                }
                catch (Exception) { }

                return m_StorageRoot;
            }

            set
            {
                m_StorageRoot = value;
                if (m_StorageRoot == null || m_StorageRoot.Length == 0)
                {
                    m_StorageRoot = DefaultStorageRoot;
                }

                if (m_StorageRoot[m_StorageRoot.Length - 1] != Path.DirectorySeparatorChar)
                {
                    m_StorageRoot += Path.DirectorySeparatorChar;
                }

                try
                {
                    byte[] buff = BitHelper.GetBytes(m_StorageRoot);
                    fixed (byte* p = buff)
                    {
                        CTrinityConfig.SetStorageRoot(p, buff.Length);
                    }
                }
                catch (Exception) { }

                if (!Directory.Exists(m_StorageRoot))
                {
                    try
                    {
                        Directory.CreateDirectory(m_StorageRoot);
                    }
                    catch (Exception)
                    {
                        ThrowCreatingStorageRootException(m_StorageRoot);
                    }
                }
            }
        }

        /// <summary>
        /// Default = 10 M
        /// </summary>
        internal int LargeObjectThreshold
        {
            get
            {
                return CTrinityConfig.CLargeObjectThreshold();
            }
            set
            {
                if (value >= 0xFFFFFF)
                {
                    ThrowLargeObjectThresholdException();
                }
                else
                {
                    CTrinityConfig.CSetLargeObjectThreshold(value);
                }
            }
        }

        /// <summary>
        /// Defragmentation frequency, Default Value = 600
        /// </summary>
        [ConfigSetting(Optional: true)]
        public int DefragInterval
        {
            get { return m_DefragInterval; }
            set { m_DefragInterval = value; CTrinityConfig.CSetGCDefragInterval(m_DefragInterval); }
        }
        #endregion
    }
}
#region tips
/*
 Rough storage profiling
+-------------------------+--------------------
  StorageCapacityProfile  |   CommittedMemory
  256M                    |   2127652K
  512M                    |   2144752K
  1G                      |   2177592K
  2G                      |   2243280K
  4G                      |   2374544K
  8G                      |   2636648K
  16G                     |   3162496K
  32G                     |   4213124K
*/
#endregion