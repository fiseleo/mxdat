import os


class ENV:
    DISABLE_TQDM = bool(os.getenv("DISABLE_TQDM"))
