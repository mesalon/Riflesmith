{
	outputs = { self, nixpkgs }:
		let
		system = "x86_64-linux";
	pkgs = nixpkgs.legacyPackages.${system};
	letmyxml2 = pkgs.runCommandNoCC "libxml2forunity" {} ''
		mkdir -p $out/lib
		ln -s ${pkgs.libxml2.out}/lib/libxml2.so $out/lib/libxml2.so.2
		'';
	libraries = with pkgs; [
		at-spi2-atk
			cairo
			fontconfig
			gtk3
			gdk-pixbuf
			glib
			libGL
			harfbuzz
			pango
			stdenv.cc.cc.lib
			udev
			libx11
			libxcursor
			libxrandr
			zlib
			libxml2
			letmyxml2
	];
	in {
		devShells.${system}.default = pkgs.mkShell {
			packages = libraries;
			NIX_LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath libraries;
			NIX_LD = pkgs.lib.fileContents "${pkgs.stdenv.cc}/nix-support/dynamic-linker";
			UNITY = "/home/mesalon/Applications/Unity/Editor/6000.3.5f1/Editor/Unity";
		};
	};
}
