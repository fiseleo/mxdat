from pathlib import Path
from zipfile import ZipFile
from .TableEncryptionService import newZipPassword


class TableZipFile(ZipFile):
    def __init__(self, file: Path) -> None:
        super().__init__(file)
        self.password = newZipPassword(file.name)

    def open(self, name: str, mode: str = "r", force_zip64: bool = False):
        return super(self.__class__, self).open(
            name, mode, pwd=self.password, force_zip64=force_zip64
        )
