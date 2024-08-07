import cloudscraper  # type: ignore
import shutil
from pathlib import Path
from tqdm import tqdm
from urllib.parse import urlencode
from local_info import local_jp_ver
from url import get_jp_version
from lib.env import ENV
from lib.filepath import FP_JP_APK
import argparse
import os

def download_jp_apk_with_ver(version: str):
    package = FP_JP_APK.stem
    code = version.split(".")[-1]
    q = {"versionCode": code, "nc": "arm64-v8a", "sv": 24}
    url = f"https://d.apkpure.com/b/XAPK/{package}?{urlencode(q)}"
    
    print(f"Constructed URL: {url}")  # Debug log
    
    scraper = cloudscraper.create_scraper()  # type: ignore
    response = scraper.get(url, stream=True)
    
    print(f"Response Status Code: {response.status_code}")  # Debug log
    
    if not response.headers.get("content-type", "").startswith("application"):
        raise RuntimeError(f"{package} v{version} is not available on apkpure.")
    
    script_path = Path(os.path.realpath(__file__))
    script_dir = script_path.parent  # Get the directory where the script is located

    apk_folder = script_dir / "APK"
    apk_folder.mkdir(exist_ok=True)  # Create APK folder if it doesn't exist
    apk_path = apk_folder / (FP_JP_APK.name + "." + version + ".xapk")  # Ensure the extension is .xapk
    
    if not apk_path.exists():
        if ENV.DISABLE_TQDM:
            with open(apk_path, "wb") as f:
                for chunk in response.iter_content(chunk_size=None):
                    if chunk:
                        f.write(chunk)
        else:
            with tqdm.wrapattr(  # type: ignore
                response.raw,
                "read",
                desc=f"Download {package} v{version}",
                total=int(response.headers.get("content-length", 0)),
            ) as r_raw:
                with open(apk_path, "wb") as f:
                    shutil.copyfileobj(r_raw, f)
    
    print(f"APK file: {apk_path}")
    
    # Save the path to a file that the C# program can read
    apk_path_file = script_dir / "apk_path.txt"
    with open(apk_path_file, "w") as path_file:
        path_file.write(str(apk_path))

    return apk_path  # Return the downloaded APK path

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("-f", dest="forcedVer", required=False)
    args = parser.parse_args()
    if args.forcedVer is None:
        # Get new version number
        new_ver = get_jp_version()
    else:
        new_ver = args.forcedVer
    # Debug log, show the new version number
    print(f"New version: {new_ver}")

    # Call the download function with the version number
    download_jp_apk_with_ver(new_ver)
