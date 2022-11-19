// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
import * as monaco from 'monaco-editor';
import React, { useEffect, useState } from 'react';
import Editor from '@monaco-editor/react';
import { createLanguageClient } from './client';
import { BaseLanguageClient, Disposable, MonacoServices } from 'monaco-languageclient';

interface Props {
  initialCode: string,
  onBicepChange: (bicepContent: string) => void,
  onJsonChange: (jsonContent: string) => void,
}

let globalService: Disposable;
let globalClient: BaseLanguageClient;

const editorOptions: monaco.editor.IStandaloneEditorConstructionOptions = {
  scrollBeyondLastLine: false,
  automaticLayout: true,
  minimap: {
    enabled: false,
  },
  insertSpaces: true,
  tabSize: 2,
  suggestSelection: 'first',
  suggest: {
    snippetsPreventQuickSuggestions: false,
    showWords: false,
  },
  'semanticHighlighting.enabled': true,
};

export const BicepEditor : React.FC<Props> = (props) => {
  const [editor, setEditor] = useState<monaco.editor.IStandaloneCodeEditor>();

  async function onEditorUpdate() {
    if (!editor) {
      return;
    }

    // @ts-expect-error: Using a private method on editor
    editor._themeService._theme.getTokenStyleMetadata = (type) => {
      // see 'monaco-editor/esm/vs/editor/standalone/common/themes.js' to understand these indices
      switch (type) {
        case 'keyword':
          return { foreground: 12 };
        case 'comment':
          return { foreground: 7 };
        case 'parameter':
          return { foreground: 2 };
        case 'property':
          return { foreground: 3 };
        case 'type':
          return { foreground: 8 };
        case 'member':
          return { foreground: 6 };
        case 'string':
          return { foreground: 5 };
        case 'variable':
          return { foreground: 4 };
        case 'operator':
          return { foreground: 9 };
        case 'function':
          return { foreground: 13 };
        case 'number':
          return { foreground: 15 };
        case 'class':
        case 'enummember':
        case 'event':
        case 'modifier':
        case 'label':
        case 'typeParameter':
        case 'macro':
        case 'interface':
        case 'enum':
        case 'regexp':
        case 'struct':
        case 'namespace':
          return { foreground: 0 };
      }
    };

    if (!globalService) {
      globalService = MonacoServices.install(editor as any);
    }
    if (!globalClient) {
      globalClient = createLanguageClient();
      await globalClient.start();
    }
    await handleOnChange(editor.getModel().getValue());
  }

  useEffect(() => { onEditorUpdate(); }, [editor]);

  const editorDidMount = (editor: monaco.editor.IStandaloneCodeEditor) => {
    setEditor(editor);
  };

  const handleOnChange = async (text: string) => {
    props.onBicepChange(text);

    const jsonContent: {output?: string} = await globalClient.sendRequest(
      "workspace/executeCommand",
      {
        command: "buildActiveCompilation",
        arguments: [{
          bicepUri: editor.getModel().uri.toString()
        }],
      }
    );
    props.onJsonChange(jsonContent.output ?? "Compilation failed!");
  }

  return <Editor
    language="bicep"
    theme="vs-dark"
    options={editorOptions}
    value={props.initialCode}
    onChange={handleOnChange}
    onMount={editorDidMount}
  />
};
