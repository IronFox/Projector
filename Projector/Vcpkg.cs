using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Projector
{
	/// <summary>
	/// vcpkg manifest-mode integration declared by a &lt;vcpkg&gt; element in a .project file.
	/// The generated .vcxproj imports vcpkg's MSBuild integration (vcpkg.props / vcpkg.targets),
	/// which restores the packages listed in vcpkg.json on build and adds their include paths and
	/// link libraries automatically.
	/// </summary>
	/// <example>
	/// &lt;vcpkg /&gt;                                  (uses the nearest vcpkg.json above the .project file)
	/// &lt;vcpkg manifest="../vcpkg.json" /&gt;
	/// &lt;vcpkg triplet="{arch}-windows-static" root="C:/dev/vcpkg" install="true" /&gt;
	/// </example>
	public class VcpkgSettings
	{
		/// <summary>
		/// Default triplet pattern. Projector always links the static CRT (/MT, /MTd), so the static triplets match.
		/// </summary>
		public const string DefaultTriplet = "{arch}-windows-static";

		/// <summary>
		/// Folder containing vcpkg.json
		/// </summary>
		public DirectoryInfo ManifestRoot { get; }
		/// <summary>
		/// Triplet pattern. {arch} is replaced by x86, x64 or arm64
		/// </summary>
		public string Triplet { get; }
		/// <summary>
		/// Explicit vcpkg root (folder containing scripts/buildsystems/msbuild/vcpkg.props), or null to auto-detect
		/// </summary>
		public DirectoryInfo? ExplicitRoot { get; }
		/// <summary>
		/// Whether MSBuild should restore (install) the manifest's packages before building
		/// </summary>
		public bool Install { get; }

		public VcpkgSettings(DirectoryInfo manifestRoot, string triplet, DirectoryInfo? explicitRoot, bool install)
		{
			ManifestRoot = manifestRoot;
			Triplet = triplet;
			ExplicitRoot = explicitRoot;
			Install = install;
		}

		/// <summary>
		/// Parses a &lt;vcpkg&gt; element. Returns null (after warning) if no manifest can be found.
		/// </summary>
		public static VcpkgSettings? Parse(XmlNode xVcpkg, DirectoryInfo projectDir, Action<string> warn)
		{
			DirectoryInfo? manifestRoot = null;
			var xManifest = xVcpkg.Attributes?.GetNamedItem("manifest");
			if (xManifest is not null && !string.IsNullOrWhiteSpace(xManifest.Value))
			{
				string full = Path.GetFullPath(Path.Combine(projectDir.FullName, xManifest.Value));
				if (Directory.Exists(full))
					manifestRoot = new DirectoryInfo(full);
				else if (File.Exists(full))
					manifestRoot = new FileInfo(full).Directory;
				if (manifestRoot is null || !File.Exists(Path.Combine(manifestRoot.FullName, "vcpkg.json")))
				{
					warn("<vcpkg>: manifest '" + xManifest.Value + "' does not point to a vcpkg.json file (resolved to '" + full + "')");
					return null;
				}
			}
			else
			{
				for (var dir = projectDir; dir is not null; dir = dir.Parent)
					if (File.Exists(Path.Combine(dir.FullName, "vcpkg.json")))
					{
						manifestRoot = dir;
						break;
					}
				if (manifestRoot is null)
				{
					warn("<vcpkg>: no vcpkg.json found in '" + projectDir.FullName + "' or any parent folder");
					return null;
				}
			}

			string triplet = DefaultTriplet;
			var xTriplet = xVcpkg.Attributes?.GetNamedItem("triplet");
			if (xTriplet is not null && !string.IsNullOrWhiteSpace(xTriplet.Value))
				triplet = xTriplet.Value.Trim();

			DirectoryInfo? root = null;
			var xRoot = xVcpkg.Attributes?.GetNamedItem("root");
			if (xRoot is not null && !string.IsNullOrWhiteSpace(xRoot.Value))
				root = new DirectoryInfo(Path.GetFullPath(Path.Combine(projectDir.FullName, xRoot.Value)));

			bool install = true;
			var xInstall = xVcpkg.Attributes?.GetNamedItem("install");
			if (xInstall is not null && bool.TryParse(xInstall.Value, out bool b))
				install = b;

			return new VcpkgSettings(manifestRoot, triplet, root, install);
		}

		/// <summary>
		/// vcpkg architecture name for a Projector platform
		/// </summary>
		public static string Arch(Platform p)
		{
			switch (p)
			{
				case Platform.x86:
					return "x86";
				case Platform.ARM:
					return "arm64";
				default:
					return "x64";
			}
		}

		public string TripletFor(Platform p) => Triplet.Replace("{arch}", Arch(p));

		private static bool IsVcpkgRoot(string? dir)
		{
			return !string.IsNullOrEmpty(dir)
				&& File.Exists(Path.Combine(dir, "scripts", "buildsystems", "msbuild", "vcpkg.props"));
		}

		/// <summary>
		/// Finds the vcpkg installation to use: the explicit root attribute, then the VCPKG_ROOT
		/// environment variable, then the copy bundled with the selected Visual Studio (VC\vcpkg).
		/// Falls back to the literal MSBuild expression $(VCPKG_ROOT), evaluated at build time.
		/// </summary>
		public string ResolveRoot(ToolsetVersion toolset, Action<string> warn)
		{
			if (ExplicitRoot is not null)
			{
				if (!IsVcpkgRoot(ExplicitRoot.FullName))
					warn("<vcpkg>: '" + ExplicitRoot.FullName + "' does not look like a vcpkg root (scripts\\buildsystems\\msbuild\\vcpkg.props is missing)");
				return ExplicitRoot.FullName;
			}
			string? env = Environment.GetEnvironmentVariable("VCPKG_ROOT");
			if (IsVcpkgRoot(env))
				return Path.GetFullPath(env!);

			// ToolsetVersion.Path points to <VS install>\Common7\IDE
			if (!string.IsNullOrEmpty(toolset.Path))
			{
				var ide = new DirectoryInfo(toolset.Path);
				var install = ide.Parent?.Parent;
				if (install is not null)
				{
					string bundled = Path.Combine(install.FullName, "VC", "vcpkg");
					if (IsVcpkgRoot(bundled))
						return bundled;
				}
			}
			warn("<vcpkg>: no vcpkg installation found (checked VCPKG_ROOT and " + toolset.VSName + "'s bundled VC\\vcpkg). The project will use $(VCPKG_ROOT) at build time.");
			return "$(VCPKG_ROOT)";
		}

		public override bool Equals(object? obj)
		{
			return obj is VcpkgSettings o
				&& string.Equals(o.ManifestRoot.FullName, ManifestRoot.FullName, StringComparison.OrdinalIgnoreCase)
				&& o.Triplet == Triplet
				&& string.Equals(o.ExplicitRoot?.FullName, ExplicitRoot?.FullName, StringComparison.OrdinalIgnoreCase)
				&& o.Install == Install;
		}

		public override int GetHashCode()
		{
			return ManifestRoot.FullName.ToLowerInvariant().GetHashCode() ^ Triplet.GetHashCode();
		}
	}
}
