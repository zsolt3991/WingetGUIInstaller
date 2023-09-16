using GithubPackageUpdater.Enums;
using System;
using Windows.ApplicationModel;

namespace WingetGUIInstaller.Utils
{

    internal static class MsixPackageExtensions
    {
#if !UNPACKAGED
        public static Version ToVersion(this PackageVersion packageVersion)
        {
            return new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }

        public static ProcessorArchitecture ToProcessorArchitecture(this Windows.System.ProcessorArchitecture processorArchitecture)
        {
            return processorArchitecture switch
            {
                Windows.System.ProcessorArchitecture.Arm => ProcessorArchitecture.ARM,
                Windows.System.ProcessorArchitecture.Arm64 => ProcessorArchitecture.ARM64,
                Windows.System.ProcessorArchitecture.X86 => ProcessorArchitecture.X86,
                Windows.System.ProcessorArchitecture.X64 => ProcessorArchitecture.X64,
                _ => throw new NotSupportedException("Processor Architecture Not Supported " + processorArchitecture)
            };
        }
#endif

        public static ProcessorArchitecture ToProcessorArchitecture(this System.Reflection.ProcessorArchitecture processorArchitecture)
        {
            return processorArchitecture switch
            {
                System.Reflection.ProcessorArchitecture.Arm => ProcessorArchitecture.ARM,
                System.Reflection.ProcessorArchitecture.X86 => ProcessorArchitecture.X86,
                System.Reflection.ProcessorArchitecture.Amd64 => ProcessorArchitecture.X64,
                _ => throw new NotSupportedException("Processor Architecture Not Supported " + processorArchitecture)
            };
        }
    }
}
