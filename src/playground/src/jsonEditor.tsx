// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
import * as monacoEditor from 'monaco-editor';
import React from 'react';
import Editor from '@monaco-editor/react';

interface JsonEditorProps {
  content: string;
}

export const JsonEditor : React.FC<JsonEditorProps> = props=> {
  const options: monacoEditor.editor.IStandaloneEditorConstructionOptions = {
    scrollBeyondLastLine: false,
    automaticLayout: true,
    minimap: {
      enabled: false,
    },
    readOnly: true,
  };

  return <Editor
    language="json"
    theme="vs-dark"
    value={props.content}
    options={options}
  />
};
