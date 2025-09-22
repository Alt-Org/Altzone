import os
import pathlib

from typing import TypeAlias
from typing import ForwardRef
from typing import Callable
import enum
from enum import Enum
from enum import Flag

import re

#{ Types

ConfigDict:   TypeAlias = dict[str, ForwardRef("Config")]
FileList:     TypeAlias = list[ForwardRef("File")]
VarDict:      TypeAlias = dict[str, str]
VarUsedDict:  TypeAlias = dict[str, list[str]]

class Config:
  def __init__(self, name: str, global_variables: VarDict):
    self.name: str = name
    self.variables: VarDict = global_variables.copy()

class ConfigParserState(Flag):
  PARSE_LINE              = enum.auto()
  PARSE_CONFIG            = enum.auto()
  PARSE_VARIABLE          = enum.auto()
  HANDLE_GLOBAL_VARIABLES = enum.auto()
  HANDLE_CONFIG_BEGIN     = enum.auto()
  HANDLE_CONFIG_END       = enum.auto()
  HANDLE_STR_LITERAL      = enum.auto()
  HANDLE_NAME             = enum.auto()
  HANDLE_VARIABLE_READ    = enum.auto()
  HANDLE_VARIABLE_WRITE   = enum.auto()
  HANDLE_FUNCTION_BEGIN   = enum.auto()
  HANDLE_FUNCTION_END     = enum.auto()
  END_OF_FILE             = enum.auto()

class FileFragmentType(Enum):
  STR = 0
  VAR = 1

class FileFragment:
  def __init__(self, frag_type: FileFragmentType, data: str):
    self.frag_type: FileFragmentType = frag_type
    self.data: str = data

class File:
  def __init__(self, name: str):
    self.name: str = name
    self.fragments: list[FileFragment] = list()

  def append(self, frag_type: FileFragmentType, string: str) -> None:
    self.fragments.append(FileFragment(frag_type, string))

class UtilStack:
  def __init__(self):
    self._list: list = list()

  def size(self) -> int:
    return len(self._list)

  def set(self, value):
    self._list[len(self._list)-1] = value

  def add(self, value):
    self._list[len(self._list)-1] += value

  def get(self) -> any:
    return self._list[len(self._list)-1]

  def push(self, value):
    self._list.append(value)

  def pop(self) -> any:
    return self._list.pop()

#} Types

#{ Logic

def main():

  print("-- SETUP INITIALIZING --\n")

  configs: ConfigDict = ConfigDict()
  if not parse_configs(configs):
    exit()

  files: FileList = FileList()
  used_variables: VarUsedDict = VarUsedDict()
  scan_files(files)

  print("-- SETUP PROCESING --\n")

  parse_files(files, used_variables)

  print("-- SETUP VALIDATING --\n")

  if not check_configs(configs, used_variables):
    exit()

  print("-- SETUP RUNNING --\n")

  generate_configs(configs, files)

  print("-- SETUP COMPILETE --")

def configs_fn_abs_path(arg: str) -> str:
  return pathlib.Path(arg).resolve().as_posix()

def parse_configs(configs: ConfigDict) -> bool:

  configs_file_name: str = "configs"

  if not os.path.isfile(configs_file_name):
    print(f"{configs_file_name} file mising")
    return False

  print("reading configs file...")
  configs_file = open("configs", "rt", encoding="utf8")

  variables_global: VarDict = VarDict()

  config_prev: Config = None
  config:      Config = None
  variable_scope: VarDict = variables_global

  functions: dict[str, Callable[[str], str]] = {
    "AbsPath" : configs_fn_abs_path
  }

  patern_whitespace:  re.Pattern = re.compile(r"[ \t]*(\n)?")
  patern_start:       re.Pattern = re.compile(r"[ \t]*(?:(#)[ \t]*)?([\w-]+)")
  patern_assign:      re.Pattern = re.compile(r"[ \t]*=")
  patern_value:       re.Pattern = re.compile(r"[ \t]*(?:(\")|([\w-]+))")
  patern_special:     re.Pattern = re.compile(r"[ \t]*([+()\n]|$)")
  patern_str_literal: re.Pattern = re.compile(r"((?:[^\n\"\\]|\\\"|\\\\)*)\"")
  sub_escape:         re.Pattern = re.compile(r"\\(.)")

  state: ConfigParserState
  line_num: int = -1
  col_num:  int = -1

  def print_variables(indent: str, label: str, var_dict: VarDict) -> None:
    var_list: list[str] = list(var_dict.keys())
    var_list.sort()
    var_len_max: int = 0

    for var in var_dict:
      var : str
      var_len: int = len(var)
      if var_len > var_len_max:
        var_len_max = var_len

    for var in var_dict:
      var : str
      print(f"{indent}{label}: {var:{var_len_max}} = \"{var_dict[var]}\"")

  def success() -> bool:
    return True

  def no_configs() -> bool:
    print("no configs defined")
    return False

  def error_syntax(ln: int) -> bool:
    print(f"syntax ERROR ON LINE {ln+1}")
    return False

  def error_config_name(name: str, ln: int) -> bool:
    print(f"dublicate config \"{name}\" ERROR ON LINE {ln+1}")
    return False

  def error_variable_name(name: str, ln: int) -> bool:
    print(f"undefined variable \"{name}\" ERROR ON LINE {ln+1}")
    return False

  def error_function_name(name: str, ln: int) -> bool:
    print(f"undefined function \"{name}\" ERROR ON LINE {ln+1}")
    return False

  while True:
    line: str = configs_file.readline()
    state = ConfigParserState.PARSE_LINE
    line_num += 1

    m = patern_whitespace.fullmatch(line, 0)
    if m:
      m: re.Match

      if m[1] == "\n":
        continue
      else:
        state = ConfigParserState.END_OF_FILE

    if ConfigParserState.PARSE_LINE in state:

      m = patern_start.match(line, 0)
      if not m:
        return error_syntax(line_num)

      m: re.Match
      col_num = m.end()

      if m[1] == '#':
        state = ConfigParserState.PARSE_CONFIG
      else:
        state = ConfigParserState.PARSE_VARIABLE

    if ConfigParserState.PARSE_CONFIG in state:

      m2 = patern_whitespace.fullmatch(line, col_num)
      if not m2:
        return error_syntax(line_num)

      m: re.Match
      config_name: str = m[2]

      if config_name in configs:
        return error_config_name(config_name, line_num)

      config_prev = config
      config = Config(config_name, variables_global)

      m2: re.Match

      if m2[1] == '\n':
        state = ConfigParserState.HANDLE_CONFIG_BEGIN | ConfigParserState.HANDLE_CONFIG_END
      else:
        state = ConfigParserState.HANDLE_CONFIG_BEGIN | ConfigParserState.END_OF_FILE

      if variable_scope is variables_global:
        state = state | ConfigParserState.HANDLE_GLOBAL_VARIABLES

    elif ConfigParserState.PARSE_VARIABLE in state:

      m2 = patern_assign.match(line, col_num)
      if not m2:
        return error_syntax(line_num)

      m2: re.Match
      col_num = m2.end()

      m: re.Match
      var_name: str = m[2]

      stack_var:   UtilStack = UtilStack()
      stack_funct: UtilStack = UtilStack()

      stack_var.push("")

      while True:

        m = patern_value.match(line, col_num)
        if not m:
          return error_syntax(line_num)

        m: re.Match
        col_num = m.end()

        if m[1] == '"':

          m = patern_str_literal.match(line, col_num)
          if not m:
            return error_syntax(line_num)

          m: re.Match
          col_num = m.end()

          stack_var.push(m[1])
          state = ConfigParserState.PARSE_VARIABLE | ConfigParserState.HANDLE_STR_LITERAL

        else:
          stack_var.push(m[2])
          state = ConfigParserState.PARSE_VARIABLE | ConfigParserState.HANDLE_NAME

        while True:

          m = patern_special.match(line, col_num)
          if not m:
            return error_syntax(line_num)

          m: re.Match
          col_num = m.end()

          match m[1]:
            case "+":
              pass
            case "(":
              state = state | ConfigParserState.HANDLE_FUNCTION_BEGIN
            case ")":
              state = state | ConfigParserState.HANDLE_FUNCTION_END
            case '\n':
              state = state | ConfigParserState.HANDLE_VARIABLE_WRITE
            case '':
              state = state | ConfigParserState.HANDLE_VARIABLE_WRITE | ConfigParserState.END_OF_FILE

          if ConfigParserState.HANDLE_FUNCTION_BEGIN in state:
            if ConfigParserState.HANDLE_NAME not in state:
              return error_syntax(line_num)
          else:
            if ConfigParserState.HANDLE_NAME in state:
              state = state | ConfigParserState.HANDLE_VARIABLE_READ

          if ConfigParserState.HANDLE_STR_LITERAL in state:

            stack_var.add(sub_escape.subn(r"\g<1>", stack_var.pop())[0])

          elif ConfigParserState.HANDLE_VARIABLE_READ in state:
            if stack_var.get() not in variable_scope:
              return error_variable_name(stack_var.get(), line_num)

            stack_var.add(variable_scope[stack_var.pop()])

          elif ConfigParserState.HANDLE_FUNCTION_BEGIN in state:
            if stack_var.get() not in functions:
              return error_function_name(stack_var.get(), line_num)

            stack_funct.push(stack_var.pop())
            stack_var.push("")

          if ConfigParserState.HANDLE_FUNCTION_END in state:
            if stack_funct.size() <= 0:
              return error_syntax(line_num)

            stack_var.add(functions[stack_funct.pop()](stack_var.pop()))

            state = ConfigParserState.PARSE_VARIABLE

          else:
            break

        if ConfigParserState.HANDLE_VARIABLE_WRITE in state:
          if stack_funct.size() > 0:
            return error_syntax(line_num)

          variable_scope[var_name] = stack_var.pop()
          break

      if ConfigParserState.END_OF_FILE in state:
        state = ConfigParserState.END_OF_FILE
      else:
        state = ConfigParserState.PARSE_VARIABLE

    if ConfigParserState.HANDLE_GLOBAL_VARIABLES in state:
      print()
      print_variables("", "global", variables_global)

    if ConfigParserState.END_OF_FILE in state:
      state = state | ConfigParserState.HANDLE_CONFIG_END

    while True:
      if ConfigParserState.HANDLE_CONFIG_END in state and config_prev:
        var_list: list[str] = list(config_prev.variables.keys())
        for var in var_list:
          var: str
          if var[0] == '_':
            config_prev.variables.pop(var)

        print_variables("  ", "var", config_prev.variables)

        config_prev = None

      if ConfigParserState.HANDLE_CONFIG_BEGIN in state and config:
        variable_scope = config.variables
        configs[config_name] = config
        print(f"\nconfig: {config.name}")

      if ConfigParserState.END_OF_FILE in state and config:
        config_prev = config
        config = None
      else:
        break

    if ConfigParserState.END_OF_FILE in state:
      break

  if len(configs) <= 0:
    return no_configs()

  print("\nconfigs file read\n")
  return success()

def scan_files(files: FileList) -> None:
  print("scaning files...")
  for file_name in os.listdir('.'):
    file_name: str

    if not os.path.isfile(file_name)        : continue
    if file_name == "setup.py" or file_name == "configs" : continue

    print(f"found {file_name}")
    files.append(File(file_name))

  print("files scanned\n")

def parse_files(files: FileList, used_variables: VarUsedDict) -> None:

  file_count:     str = str(len(files))
  file_count_len: int = len(file_count)
  file_num:       int = 0

  patern_variable: re.Pattern = re.compile(r"%([\w-]*)%")

  print("reading files...")

  for file_obj in files:
    file_obj: File

    used_variables_infile: set[str] = set()

    file_num += 1
    print(f"reading [{file_num:{file_count_len}}/{file_count}] {file_obj.name}")

    file = open(file_obj.name, "rt", encoding="utf8")
    file_str: str = file.read()
    fragment_begin: int = 0
    fragment_end:   int = 0

    while True:
      m = patern_variable.search(file_str, fragment_begin)

      value: str = '\0'

      if m:
        m: re.Match

        fragment_end = m.start()
        value        = m[1]

      else:
        fragment_end = len(file_str)

      file_obj.append(FileFragmentType.STR, file_str[fragment_begin:fragment_end])

      if value != '\0':
        if value == '':
          file_obj.append(FileFragmentType.STR, "%")
        else:
          file_obj.append(FileFragmentType.VAR, value)
          used_variables_infile.add(value)
        fragment_begin = m.end()
      else:
        break

    file.close()

    for variable in used_variables_infile:
      ls: list[str]
      if variable in used_variables:
        ls: list[str] = used_variables[variable]
      else:
        ls: list[str] = list()
        used_variables[variable] = ls
      ls.append(file_obj.name)

  print("files read\n")

def check_configs(configs: ConfigDict, used_variables: VarUsedDict) -> bool:

  is_success: bool = True

  config_count:     str = str(len(configs))
  config_count_len: int = len(config_count)
  config_num:       int = 0

  print("checking configs...")

  for config in configs.values():
    config: Config
    config_num += 1
    print(f"checking [{config_num:{config_count_len}}/{config_count}] {config.name}")

    for variable in used_variables:
      variable: str
      if variable not in config.variables:
        is_success = False
        print(f"variable \"{variable}\" not defined")
        print("used in")
        for file_name in used_variables[variable]:
          file_name: str
          print(f"  file: {file_name}")
        print()

  if is_success:
    print("configs checked\n")

  return is_success

def generate_configs(configs: ConfigDict, files: FileList):

  config_count:     str = str(len(configs))
  config_count_len: int = len(config_count)
  config_num:       int = 0

  file_count:     str = str(len(files))
  file_count_len: int = len(file_count)
  file_num:       int = 0
  file_padding:   int = 0

  for file in files:
    file: File
    file_name_len: int = len(file.name)
    if file_name_len > file_padding:
      file_padding = file_name_len

  print("generating configs...\n")

  for config in configs.values():
    config: Config
    config_num += 1

    config_path: str = f"../{config.name}"

    print(f"[generating] config [{config_num:{config_count_len}}/{config_count}] {config_path}")

    os.makedirs(config_path, exist_ok=True)

    file_num: int = 0
    for file_obj in files:
      file_obj: File
      file_num += 1

      file_path: str = f"{config_path}/{file_obj.name}"

      print(f"[generating] ./{file_obj.name:{file_padding}} --[{file_num:{file_count_len}}/{file_count}]-> {file_path}")

      with open(file_path, "wt", encoding="utf8") as file:
        for frag in file_obj.fragments:
          frag: FileFragment
          match frag.frag_type:
            case FileFragmentType.STR:
              file.write(frag.data)
            case FileFragmentType.VAR:
              file.write(config.variables[frag.data])

    print()

  print("configs generated\n")

main()

#} Logic
