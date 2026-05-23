import { useEffect, useState, type FormEvent } from 'react';
import { Eye, Plus, Search, Users } from 'lucide-react';
import { msmsApi } from '../../api/msmsApi';
import { getErrorMessage } from '../../api/client';
import type { ClassDetails, ClassItem, SubjectItem, TeacherAssignment, TeacherItem } from '../../types';
import { SCHOOL_SECTIONS } from '../../types';
import { matchesSearch } from '../../utils/search';

const SECTION_ORDER = [1, 2, 3] as const;
import {
  Alert, Button, Card, CardBody, CardHeader, EmptyState, Input, Modal,
  PageHeader, Select, TableWrap, Tabs, Td, Th,
} from '../../components/ui';

type Tab = 'classes' | 'subjects' | 'assignments';

export function SchoolPage() {
  const [tab, setTab] = useState<Tab>('classes');
  const [classes, setClasses] = useState<ClassItem[]>([]);
  const [subjects, setSubjects] = useState<SubjectItem[]>([]);
  const [teachers, setTeachers] = useState<TeacherItem[]>([]);
  const [assignments, setAssignments] = useState<TeacherAssignment[]>([]);
  const [open, setOpen] = useState(false);
  const [classDetails, setClassDetails] = useState<ClassDetails | null>(null);
  const [search, setSearch] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const [classForm, setClassForm] = useState({ name: '', gradeLevel: '', section: '', academicYear: '2025-2026' });
  const [subjectForm, setSubjectForm] = useState({ name: '', code: '' });
  const [assignForm, setAssignForm] = useState({ teacherId: '', subjectId: '', classId: '' });

  const load = () => {
    msmsApi.school.listClasses().then((r) => setClasses(r.data));
    msmsApi.school.listSubjects().then((r) => setSubjects(r.data));
    msmsApi.people.listTeachers().then((r) => setTeachers(r.data));
    msmsApi.school.listAssignments().then((r) => setAssignments(r.data));
  };

  useEffect(() => { load(); }, []);

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      if (tab === 'classes') {
        await msmsApi.school.createClass({ ...classForm, section: classForm.section || null });
      } else if (tab === 'subjects') {
        await msmsApi.school.createSubject(subjectForm);
      } else {
        await msmsApi.school.assignTeacher(assignForm);
      }
      setOpen(false);
      load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const modalTitle = tab === 'classes' ? 'Add Class' : tab === 'subjects' ? 'Add Subject' : 'Assign Teacher';

  const openClassDetails = async (id: string) => {
    setError('');
    try {
      const res = await msmsApi.school.getClass(id);
      setClassDetails(res.data);
    } catch (err) {
      setError(getErrorMessage(err));
    }
  };
  const filteredClasses = classes.filter((c) =>
    matchesSearch(search, [c.name, c.gradeLevel, c.section, c.academicYear, SCHOOL_SECTIONS[c.schoolSection]])
  );
  const filteredSubjects = subjects.filter((s) => matchesSearch(search, [s.name, s.code]));
  const filteredAssignments = assignments.filter((a) =>
    matchesSearch(search, [a.teacherName, a.subjectName, a.className])
  );

  return (
    <>
      <PageHeader
        title="Classes & Subjects"
        description="School structure and teacher assignments"
        action={<Button onClick={() => setOpen(true)}><Plus className="h-4 w-4" /> Add</Button>}
      />

      <div className="mb-6">
        <Tabs
          tabs={[
            { id: 'classes' as Tab, label: 'Classes' },
            { id: 'subjects' as Tab, label: 'Subjects' },
            { id: 'assignments' as Tab, label: 'Assignments' },
          ]}
          active={tab}
          onChange={setTab}
        />
      </div>

      <Card className="mb-6">
        <CardBody>
          <label className="relative block">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search class, grade, section, subject, teacher..."
              className="w-full rounded-xl border border-slate-200 bg-slate-50/80 py-2.5 pl-9 pr-3 text-sm outline-none transition focus:border-indigo-500 focus:bg-white focus:ring-4 focus:ring-indigo-500/10"
            />
          </label>
        </CardBody>
      </Card>

      {tab === 'classes' && (
        <>
          {SECTION_ORDER.map((section) => {
            const sectionClasses = filteredClasses
              .filter((c) => c.schoolSection === section)
              .sort((a, b) => Number(a.gradeLevel) - Number(b.gradeLevel));
            return (
              <Card key={section} className="mb-6">
                <CardHeader
                  title={SCHOOL_SECTIONS[section]}
                  subtitle={`Grades ${section === 1 ? '1–4' : section === 2 ? '5–8' : '9–12'} · ${sectionClasses.length} classes`}
                />
                <CardBody className="p-0">
                  {sectionClasses.length === 0 ? (
                    <EmptyState message="No classes in this section." />
                  ) : (
                    <TableWrap>
                      <thead><tr><Th>Name</Th><Th>Grade</Th><Th>Room</Th><Th>Year</Th><Th>Info</Th></tr></thead>
                      <tbody>
                        {sectionClasses.map((c) => (
                          <tr key={c.id}>
                            <Td className="font-medium">{c.name}</Td>
                            <Td>Grade {c.gradeLevel}</Td>
                            <Td>{c.section ?? '—'}</Td>
                            <Td>{c.academicYear}</Td>
                            <Td>
                              <Button size="sm" variant="secondary" onClick={() => openClassDetails(c.id)}>
                                <Eye className="h-4 w-4" /> View
                              </Button>
                            </Td>
                          </tr>
                        ))}
                      </tbody>
                    </TableWrap>
                  )}
                </CardBody>
              </Card>
            );
          })}
        </>
      )}

      {tab === 'subjects' && (
        <Card>
          <CardHeader title="Subjects" subtitle={`${filteredSubjects.length} of ${subjects.length} subjects`} />
          <CardBody className="p-0">
            {filteredSubjects.length === 0 ? <EmptyState message="No subjects." /> : (
              <TableWrap>
                <thead><tr><Th>Name</Th><Th>Code</Th></tr></thead>
                <tbody>
                  {filteredSubjects.map((s) => (
                    <tr key={s.id}>
                      <Td className="font-medium">{s.name}</Td>
                      <Td>{s.code}</Td>
                    </tr>
                  ))}
                </tbody>
              </TableWrap>
            )}
          </CardBody>
        </Card>
      )}

      {tab === 'assignments' && (
        <Card>
          <CardHeader title="Teacher Assignments" subtitle={`${filteredAssignments.length} of ${assignments.length} assignments`} />
          <CardBody className="p-0">
            {filteredAssignments.length === 0 ? <EmptyState message="No assignments." /> : (
              <TableWrap>
                <thead><tr><Th>Teacher</Th><Th>Subject</Th><Th>Class</Th></tr></thead>
                <tbody>
                  {filteredAssignments.map((a, i) => (
                    <tr key={`${a.teacherId}-${a.subjectId}-${a.classId}-${i}`}>
                      <Td>{a.teacherName}</Td>
                      <Td>{a.subjectName}</Td>
                      <Td>{a.className}</Td>
                    </tr>
                  ))}
                </tbody>
              </TableWrap>
            )}
          </CardBody>
        </Card>
      )}

      <Modal open={open} onClose={() => setOpen(false)} title={modalTitle}>
        {error && <Alert message={error} />}
        <form onSubmit={submit} className="space-y-3">
          {tab === 'classes' && (
            <>
              <Input label="Name" value={classForm.name} onChange={(e) => setClassForm({ ...classForm, name: e.target.value })} required />
              <Input label="Grade Level" value={classForm.gradeLevel} onChange={(e) => setClassForm({ ...classForm, gradeLevel: e.target.value })} required />
              <Input label="Section" value={classForm.section} onChange={(e) => setClassForm({ ...classForm, section: e.target.value })} />
              <Input label="Academic Year" value={classForm.academicYear} onChange={(e) => setClassForm({ ...classForm, academicYear: e.target.value })} required />
            </>
          )}
          {tab === 'subjects' && (
            <>
              <Input label="Name" value={subjectForm.name} onChange={(e) => setSubjectForm({ ...subjectForm, name: e.target.value })} required />
              <Input label="Code" value={subjectForm.code} onChange={(e) => setSubjectForm({ ...subjectForm, code: e.target.value })} required />
            </>
          )}
          {tab === 'assignments' && (
            <>
              <Select label="Teacher" value={assignForm.teacherId} onChange={(e) => setAssignForm({ ...assignForm, teacherId: e.target.value })} required>
                <option value="">— Select —</option>
                {teachers.map((t) => <option key={t.id} value={t.id}>{t.firstName} {t.lastName}</option>)}
              </Select>
              <Select label="Subject" value={assignForm.subjectId} onChange={(e) => setAssignForm({ ...assignForm, subjectId: e.target.value })} required>
                <option value="">— Select —</option>
                {subjects.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
              </Select>
              <Select label="Class" value={assignForm.classId} onChange={(e) => setAssignForm({ ...assignForm, classId: e.target.value })} required>
                <option value="">— Select —</option>
                {classes.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
              </Select>
            </>
          )}
          <span className="flex justify-end gap-2 pt-2">
            <Button type="button" variant="secondary" onClick={() => setOpen(false)}>Cancel</Button>
            <Button type="submit" loading={loading}>Save</Button>
          </span>
        </form>
      </Modal>

      <Modal open={!!classDetails} onClose={() => setClassDetails(null)} title="Class Information" wide>
        {classDetails && (
          <div className="space-y-5">
            <div className="rounded-2xl bg-gradient-to-br from-indigo-600 to-slate-900 p-5 text-white">
              <p className="text-sm text-indigo-200">{SCHOOL_SECTIONS[classDetails.schoolSection]}</p>
              <h3 className="mt-1 text-2xl font-bold">{classDetails.name}</h3>
              <div className="mt-4 grid gap-3 sm:grid-cols-3">
                <div className="rounded-xl bg-white/10 p-3">
                  <p className="text-xs text-indigo-200">Students</p>
                  <p className="text-2xl font-bold">{classDetails.studentCount}</p>
                </div>
                <div className="rounded-xl bg-white/10 p-3">
                  <p className="text-xs text-indigo-200">Subjects</p>
                  <p className="text-2xl font-bold">{classDetails.subjectCount}</p>
                </div>
                <div className="rounded-xl bg-white/10 p-3">
                  <p className="text-xs text-indigo-200">Academic year</p>
                  <p className="text-lg font-bold">{classDetails.academicYear}</p>
                </div>
              </div>
            </div>
            <section>
              <h4 className="mb-2 flex items-center gap-2 font-semibold text-slate-900"><Users className="h-4 w-4" /> Students</h4>
              {classDetails.students.length === 0 ? <EmptyState message="No students in this class." /> : (
                <div className="grid gap-2 sm:grid-cols-2">
                  {classDetails.students.map((s) => (
                    <div key={s.id} className="flex items-center gap-3 rounded-xl border border-slate-100 px-3 py-2">
                      {s.photoUrl && <img src={s.photoUrl} alt="" className="h-9 w-9 rounded-full object-cover" />}
                      <span className="text-sm"><b>{s.firstName} {s.lastName}</b><br /><span className="text-slate-500">{s.studentNumber}</span></span>
                    </div>
                  ))}
                </div>
              )}
            </section>
            <section>
              <h4 className="mb-2 font-semibold text-slate-900">Subjects and teachers</h4>
              {classDetails.subjects.length === 0 ? <EmptyState message="No subjects assigned." /> : (
                <div className="grid gap-2 sm:grid-cols-2">
                  {classDetails.subjects.map((s) => (
                    <div key={`${s.subjectId}-${s.teacherId}`} className="rounded-xl border border-slate-100 px-4 py-3 text-sm">
                      <b>{s.subjectName}</b>
                      <span className="block text-slate-500">{s.teacherName}</span>
                    </div>
                  ))}
                </div>
              )}
            </section>
          </div>
        )}
      </Modal>
    </>
  );
}
