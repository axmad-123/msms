import { useEffect, useState, type FormEvent } from 'react';
import { Download, Plus, Printer } from 'lucide-react';
import { msmsApi } from '../../api/msmsApi';
import { getErrorMessage } from '../../api/client';
import type { ClassItem, ExamCatalogItem, ExamResult, SubjectItem } from '../../types';
import { EXAM_TYPES } from '../../types';
import {
  Alert, Button, Card, CardBody, CardHeader, EmptyState, Input, Modal,
  PageHeader, Select, TableWrap, Td, Th,
} from '../../components/ui';

export function ExamsPage() {
  const [catalog, setCatalog] = useState<ExamCatalogItem[]>([]);
  const [classes, setClasses] = useState<ClassItem[]>([]);
  const [subjects, setSubjects] = useState<SubjectItem[]>([]);
  const [results, setResults] = useState<ExamResult[]>([]);
  const [open, setOpen] = useState(false);
  const [form, setForm] = useState({ examType: '1', name: '', academicYear: '2025-2026' });
  const [view, setView] = useState({ classId: '', subjectId: '', examType: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const load = () => msmsApi.exams.listCatalog().then((r) => setCatalog(r.data));

  useEffect(() => {
    load();
    msmsApi.school.listClasses().then((r) => setClasses(r.data));
    msmsApi.school.listSubjects().then((r) => setSubjects(r.data));
  }, []);

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      await msmsApi.exams.createCatalog({
        examType: Number(form.examType),
        name: form.name,
        academicYear: form.academicYear,
      });
      setOpen(false);
      load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const loadResults = () => {
    if (!view.classId || !view.subjectId) return;
    msmsApi.exams
      .classSubjectResults(view.classId, view.subjectId, view.examType ? Number(view.examType) : undefined)
      .then((r) => setResults(r.data));
  };

  const exportCsv = async () => {
    if (!view.classId || !view.subjectId) return;
    try {
      const blob = await msmsApi.exams.downloadExport(
        view.classId,
        view.subjectId,
        view.examType ? Number(view.examType) : undefined
      );
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'exam-results.csv';
      a.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      setError(getErrorMessage(err));
    }
  };

  const printResults = () => window.print();

  return (
    <>
      <PageHeader
        title="Exams"
        description="Exam catalog and class results"
        action={<Button onClick={() => setOpen(true)}><Plus className="h-4 w-4" /> Add Exam</Button>}
      />

      <Card className="mb-6">
        <CardHeader title="Exam Catalog" subtitle={`${catalog.length} exams`} />
        <CardBody className="p-0">
          {catalog.length === 0 ? (
            <EmptyState message="No exams in catalog." />
          ) : (
            <TableWrap>
              <thead><tr><Th>Name</Th><Th>Type</Th><Th>Year</Th></tr></thead>
              <tbody>
                {catalog.map((e) => (
                  <tr key={e.id}>
                    <Td className="font-medium">{e.name}</Td>
                    <Td>{EXAM_TYPES[e.examType] ?? e.examType}</Td>
                    <Td>{e.academicYear}</Td>
                  </tr>
                ))}
              </tbody>
            </TableWrap>
          )}
        </CardBody>
      </Card>

      <Card>
        <CardHeader title="View Results by Class" />
        <CardBody className="space-y-4">
          <span className="grid gap-4 sm:grid-cols-4">
            <Select label="Class" value={view.classId} onChange={(e) => setView({ ...view, classId: e.target.value })}>
              <option value="">— Select —</option>
              {classes.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
            </Select>
            <Select label="Subject" value={view.subjectId} onChange={(e) => setView({ ...view, subjectId: e.target.value })}>
              <option value="">— Select —</option>
              {subjects.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
            </Select>
            <Select label="Exam Type" value={view.examType} onChange={(e) => setView({ ...view, examType: e.target.value })}>
              <option value="">All</option>
              {Object.entries(EXAM_TYPES).map(([k, v]) => (
                <option key={k} value={k}>{v}</option>
              ))}
            </Select>
            <span className="flex flex-wrap items-end gap-2">
              <Button onClick={loadResults}>Load</Button>
              {results.length > 0 && (
                <>
                  <Button type="button" variant="secondary" onClick={exportCsv}>
                    <Download className="h-4 w-4" /> Export CSV
                  </Button>
                  <Button type="button" variant="secondary" onClick={printResults}>
                    <Printer className="h-4 w-4" /> Print
                  </Button>
                </>
              )}
            </span>
          </span>
          {results.length > 0 && (
            <TableWrap>
              <thead><tr><Th>Student</Th><Th>Marks</Th><Th>Grade</Th></tr></thead>
              <tbody>
                {results.map((r) => (
                  <tr key={r.id}>
                    <Td>{r.studentName}</Td>
                    <Td>{r.marks} / {r.maxMarks}</Td>
                    <Td>{r.grade}</Td>
                  </tr>
                ))}
              </tbody>
            </TableWrap>
          )}
        </CardBody>
      </Card>

      <Modal open={open} onClose={() => setOpen(false)} title="Add Exam">
        {error && <Alert message={error} />}
        <form onSubmit={submit} className="space-y-3">
          <Select label="Type" value={form.examType} onChange={(e) => setForm({ ...form, examType: e.target.value })}>
            {Object.entries(EXAM_TYPES).map(([k, v]) => (
              <option key={k} value={k}>{v}</option>
            ))}
          </Select>
          <Input label="Name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
          <Input label="Academic Year" value={form.academicYear} onChange={(e) => setForm({ ...form, academicYear: e.target.value })} required />
          <span className="flex justify-end gap-2 pt-2">
            <Button type="button" variant="secondary" onClick={() => setOpen(false)}>Cancel</Button>
            <Button type="submit" loading={loading}>Save</Button>
          </span>
        </form>
      </Modal>
    </>
  );
}
